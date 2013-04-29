using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using TicTacTotalDomination.Util.DataServices;
using TicTacTotalDomination.Util.Games;
using TicTacTotalDomination.Util.Models;

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
                    }
                }
            }
        }

        void aiWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            AIAttentionRequiredResult aiGameAttentionRequired = (AIAttentionRequiredResult)e.Argument;
            GameState state = TicTacToeHost.Instance.GetGameState(aiGameAttentionRequired.GameId, aiGameAttentionRequired.PlayerId);
            int[,] aiBoard = new int[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    aiBoard[i, j] = state.GameBoard[i][j] == null ? 0 : state.GameBoard[i][j].Value;
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

            TicTacToeHost.Instance.Move(gameMove);

            using (IGameDataService gameDataService = new GameDataService())
            {
                AIGame aiGame = gameDataService.GetAIGame(aiGameAttentionRequired.GameId);
                gameDataService.Attach(aiGame);
                aiGame.EvaluatingMove = false;
                gameDataService.Save();
            }
        }
    }
}
