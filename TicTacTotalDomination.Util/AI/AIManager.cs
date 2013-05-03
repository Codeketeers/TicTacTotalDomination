using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using TicTacTotalDomination.Util.Caching;
using TicTacTotalDomination.Util.DataServices;
using TicTacTotalDomination.Util.Games;
using TicTacTotalDomination.Util.Logging;
using TicTacTotalDomination.Util.Models;
using TicTacTotalDomination.Util.NetworkCommunication;

namespace TicTacTotalDomination.Util.AI
{
    public class AIManager
    {
        private Timer aiTimer = null;

        private static Lazy<AIManager> _Instance = new Lazy<AIManager>(() => new AIManager());
        public static AIManager Instance { get { return _Instance.Value; } }
        private AIManager() 
        {
            this.aiTimer = new Timer();
            this.aiTimer.Elapsed += aiTimer_Elapsed;
            this.aiTimer.Interval = new TimeSpan(0, 0, 1).TotalMilliseconds;
        }

        public void StartMonitoring()
        {
            this.aiTimer.Start();
        }

        public void StopMonitoring()
        {
            this.aiTimer.Stop();
        }

        void aiTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            using(IGameDataService gameDataService = new GameDataService())
            {
                IEnumerable<AIAttentionRequiredResult> aiGames = gameDataService.GetAIGamesRequiringAttention();

                //If we have any Games that the AI needs to play, loop through them all.
                //We will notify the AI to play on seperate threads.
                if (aiGames.Any())
                {
                    foreach(var aiGame in aiGames)
                    {
                        BackgroundWorker aiWorker = new BackgroundWorker();
                        aiWorker.DoWork += aiWorker_DoWork;
                        aiWorker.RunWorkerAsync(aiGame);
                        aiWorker.RunWorkerCompleted += (cs,ce) =>
                            {
                                if (ce.Error != null)
                                {
                                    Exception ex = ce.Error;
                                    while (ex.InnerException != null)
                                    {
                                        ex = ex.InnerException;
                                    }

                                    Logger.Instance.Log("CentralServerCommunicationError", string.Format("GameId:{0}|Error:{1}",aiGame.GameId, ex.Message), ce.Error.StackTrace);
                                    using (IGameDataService dataService = new GameDataService())
                                    {
                                        Match match = dataService.GetMatch(null, aiGame.GameId);
                                        CentralServerSession session = dataService.GetCentralServerSession(null, null, aiGame.GameId);
                                        Player tttdPlayer = dataService.GetPlayer(match.PlayerOneId);
                                        GameConfiguration config = GameConfigCache.Instance.GetConfig(match.MatchId);

                                        dataService.EndGame(aiGame.GameId, match.PlayerOneId == aiGame.PlayerId ? match.PlayerTwoId : match.PlayerOneId);
                                        dataService.Save();

                                        if (config.GameType == GameType.Network)
                                        {
                                            MoveRequest challengeRequest = new MoveRequest();
                                            challengeRequest.GameId = session.CentralServerGameId.Value;
                                            challengeRequest.PlayerName = tttdPlayer.PlayerName;
                                            challengeRequest.X = 0;
                                            challengeRequest.Y = 0;
                                            challengeRequest.Flags = CentralServerCommunicationChannel.GetStatus(StatusFlag.AcceptLoss);

                                            CentralServerCommunicationChannel.Instance.PostMove(challengeRequest, match.CurrentGameId.Value, match.MatchId);
                                        }
                                    }
                                }
                            };
                    }
                }
            }
        }

        void aiWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            AIAttentionRequiredResult aiGameAttentionRequired = (AIAttentionRequiredResult)e.Argument;
            GameState state = TicTacToeHost.Instance.GetGameState(aiGameAttentionRequired.GameId, aiGameAttentionRequired.PlayerId);
            int[,] aiBoard = new int[3, 3];
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    aiBoard[x, y] = state.GameBoard[x][y] == null ? 0 : state.GameBoard[x][y].Value;
                }
            }
            GameBoard ai = new GameBoard(aiBoard);
            Move aiMove = ai.GetMove(1);
            Games.Move gameMove = new Games.Move();
            gameMove.GameId = aiGameAttentionRequired.GameId;
            gameMove.PlayerId = aiGameAttentionRequired.PlayerId;
            gameMove.OriginX = aiMove.OriginX;
            gameMove.OriginY = aiMove.OriginY;
            gameMove.X = aiMove.X;
            gameMove.Y = aiMove.Y;

            var moveReuslt = TicTacToeHost.Instance.Move(gameMove);
            if (moveReuslt != MoveResult.Valid)
            {
                //using(IGameDataService dataService = new GameDataService())
                //{
                //    Match match = dataService.GetMatch(null, aiGameAttentionRequired.GameId);
                //    Player tttdPlayer = dataService.GetPlayer(aiGameAttentionRequired.PlayerId);
                //    CentralServerSession session = dataService.GetCentralServerSession(null, null, aiGameAttentionRequired.GameId);

                //    dataService.EndGame(aiGameAttentionRequired.GameId, match.PlayerOneId == aiGameAttentionRequired.PlayerId ? match.PlayerTwoId : match.PlayerOneId);

                //    MoveRequest challengeRequest = new MoveRequest();
                //    challengeRequest.GameId = session.CentralServerGameId.Value;
                //    challengeRequest.PlayerName = tttdPlayer.PlayerName;
                //    challengeRequest.X = 0;
                //    challengeRequest.Y = 0;
                //    challengeRequest.Flags = CentralServerCommunicationChannel.GetStatus(StatusFlag.AcceptLoss);
                //    CentralServerCommunicationChannel.Instance.PostMove(challengeRequest, match.CurrentGameId.Value, match.MatchId);
                //}
            }

            using (IGameDataService gameDataService = new GameDataService())
            {
                AIGame aiGame = gameDataService.GetAIGame(aiGameAttentionRequired.GameId, aiGameAttentionRequired.PlayerId);
                gameDataService.Attach(aiGame);
                aiGame.EvaluatingMove = false;
                gameDataService.Save();
            }
        }
    }
}
