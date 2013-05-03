using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using TicTacTotalDomination.Util.DataServices;
using TicTacTotalDomination.Util.Games;
using TicTacTotalDomination.Util.Logging;
using TicTacTotalDomination.Util.Models;
using TicTacTotalDomination.Util.Serialization;

namespace TicTacTotalDomination.Util.NetworkCommunication
{
    public class CentralServerCommunicationChannel : ICommunicationChannel
    {
        public const string STATUS_WINNING_MOVE = "winning move";
        public const string STATUS_DRAW_MOVE = "draw move";
        public const string STATUS_CHALLENGE_WIN = "challenge win";
        public const string STATUS_CHALLENGE_MOVE = "challenge move";
        public const string STATUS_ACCEPT_DRAW = "accept draw";
        public const string STATUS_ACCEPT_LOSS = "accept loss";

        private static Lazy<CentralServerCommunicationChannel> _Instance = new Lazy<CentralServerCommunicationChannel>(() => new CentralServerCommunicationChannel());
        public static CentralServerCommunicationChannel Instance { get { return _Instance.Value; } }
        private CentralServerCommunicationChannel() { }

        private bool disposed = false;

        public void StartWatchingForGameEvents()
        {
            TicTacToeHost.Instance.PlayerChallenge += Instance_PlayerChallenge;
            TicTacToeHost.Instance.PlayerMove += Instance_PlayerMove;
        }

        void ICommunicationChannel.ChallengePlayer(int matchId)
        {
            using (IGameDataService dataService = new GameDataService())
            {
                //We need to create a session even if the challenge isn't accepted.
                Match match = dataService.GetMatch(matchId, null);
                dataService.CreateCentralServerSession(match.CurrentGameId.Value);

                Player player = dataService.GetPlayer(match.PlayerOneId);
                Player opponent = dataService.GetPlayer(match.PlayerTwoId);
                Game game = dataService.GetGame(match.CurrentGameId.Value);

                var requestData = new ChallengeRequest();
                requestData.PlayerName = player.PlayerName;
                requestData.OpponentName = opponent.PlayerName;

                string requestJSON = JsonSerializer.SerializeToJSON<ChallengeRequest>(requestData);
                var requestConfig = new ServerRequestConfig();
                requestConfig.Url = string.Format("{0}/ServerPairing.php", ConfigurationManager.AppSettings["CentralServerUrl"]);
                requestConfig.RequestData = requestJSON;
                requestConfig.GameId = game.GameId;
                requestConfig.MatchId = game.MatchId;
                requestConfig.ResponseAction = new Action<string, int>(ChallengePlayerCompleted);

                this.PerformServerRequest(requestConfig);
            }
        }

        void ICommunicationChannel.PostMove(int matchId, int x, int y, PlayMode mode)
        {
            using (IGameDataService dataService = new GameDataService())
            {
                Match match = dataService.GetMatch(matchId, null);
                Player player = dataService.GetPlayer(match.PlayerOneId);
                GameState state = TicTacToeHost.Instance.GetGameState(match.CurrentGameId.Value, player.PlayerId);

                int moveCount = dataService.Repository.GetGameMoves().Where(mv => mv.GameId == match.CurrentGameId.Value).GroupBy(mv => mv.MoveDate).Count();

                string flag;
                if (state.Mode == PlayMode.DeathMatch && moveCount == 9)
                {
                    flag = CentralServerCommunicationChannel.GetStatus(StatusFlag.DrawMove);
                }
                else if (state.Mode == PlayMode.DeathMatch && moveCount == 10)
                {
                    flag = CentralServerCommunicationChannel.GetStatus(StatusFlag.AcceptDraw);
                }
                else if (state.Mode == PlayMode.Won)
                {
                    flag = CentralServerCommunicationChannel.GetStatus(StatusFlag.WinningMove);
                }
                else
                {
                    flag = CentralServerCommunicationChannel.GetStatus(StatusFlag.None);
                }

                var requestData = new MoveRequest();
                requestData.PlayerName = player.PlayerName;
                requestData.GameId = -1;
                requestData.Flags = flag;
                requestData.X = x;
                requestData.Y = y;

                CentralServerSession session = dataService.GetCentralServerSession(null, null, match.CurrentGameId.Value);
                if (session != null)
                    requestData.GameId = session.CentralServerGameId.Value;

                this.PostMove(requestData, match.CurrentGameId.Value, match.MatchId);
            }
        }

        public void PostMove(MoveRequest request, int gameId, int matchId)
        {
            string requestJSON = JsonSerializer.SerializeToJSON<MoveRequest>(request);
            var requestConfig = new ServerRequestConfig();
            requestConfig.Url = string.Format("{0}/play.php", ConfigurationManager.AppSettings["CentralServerUrl"]);
            requestConfig.RequestData = requestJSON;
            requestConfig.GameId = gameId;
            requestConfig.MatchId = matchId;
            requestConfig.ResponseAction = new Action<string, int>(PostMoveCompleted);

            this.PerformServerRequest(requestConfig);
        }

        private void PerformServerRequest(ServerRequestConfig config)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, args) =>
                {
                    var workerConfig = (ServerRequestConfig)args.Argument;
                    //"http://centralserver.codeketeers.com/ServerPairing.php?challenge=James&from=Anthony"
                    JavaScriptSerializer dataSerailizer = new JavaScriptSerializer();
                    string queryString = string.Join("&", dataSerailizer.Deserialize<Dictionary<string, string>>(config.RequestData).Where(kv => !string.IsNullOrEmpty(kv.Value)).Select(kv => string.Format("{0}={1}", kv.Key, HttpUtility.UrlEncode(kv.Value != null ? kv.Value : string.Empty))));
                    string fullUrl = string.Format("{0}?{1}", config.Url, queryString);
                    Logger.Instance.Log("ServerRequest", string.Format("GameId:{0}|MatchId:{1}|Request:{2}", workerConfig.GameId, workerConfig.MatchId, fullUrl), JsonSerializer.SerializeToJSON(workerConfig));
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fullUrl);
                    request.ContentLength = 0;

                    var httpResponse = (HttpWebResponse)request.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        Logger.Instance.Log("ServerResponse", string.Format("GameId:{0}|MatchId:{1}|Request:{2}", workerConfig.GameId, workerConfig.MatchId, fullUrl), result);
                        workerConfig.ResponseAction(result, workerConfig.MatchId);
                    }
                };
            worker.RunWorkerAsync(config);
            worker.RunWorkerCompleted += (cs, ce) =>
            {
                if (ce.Error != null)
                {
                    Exception ex = ce.Error;
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                    }

                    Logger.Instance.Log("CentralServerCommunicationError", string.Format("GameId:{0}|MatchId:{1}|Error:{2}",config.GameId,config.MatchId, ex.Message), ce.Error.StackTrace);
                    using (IGameDataService dataService = new GameDataService())
                    {
                        dataService.EndGame(config.GameId, null);
                        dataService.Save();

                        Match match = dataService.GetMatch(config.MatchId, null);
                        CentralServerSession session = dataService.GetCentralServerSession(null, null, config.GameId);
                        Player tttdPlayer = dataService.GetPlayer(match.PlayerOneId);

                        MoveRequest challengeRequest = new MoveRequest();
                        challengeRequest.GameId = session.CentralServerGameId.Value;
                        challengeRequest.PlayerName = tttdPlayer.PlayerName;
                        challengeRequest.X = 0;
                        challengeRequest.Y = 0;
                        challengeRequest.Flags = CentralServerCommunicationChannel.GetStatus(StatusFlag.ChallengeMove);

                        CentralServerCommunicationChannel.Instance.PostMove(challengeRequest, match.CurrentGameId.Value, match.MatchId);
                    }
                }
            };
        }

        public static StatusFlag ParseStatus(string status)
        {
            switch(status)
            {
                case STATUS_ACCEPT_LOSS:
                    return StatusFlag.AcceptLoss;
                case STATUS_CHALLENGE_MOVE:
                    return StatusFlag.ChallengeMove;
                case STATUS_CHALLENGE_WIN:
                    return StatusFlag.ChallengeWin;
                case STATUS_DRAW_MOVE:
                    return StatusFlag.DrawMove;
                case STATUS_WINNING_MOVE:
                    return StatusFlag.WinningMove;
                case STATUS_ACCEPT_DRAW:
                    return StatusFlag.AcceptDraw;
                default:
                    return StatusFlag.None;
            }
        }

        public static string GetStatus(StatusFlag status)
        {
            switch (status)
            {
                case StatusFlag.AcceptLoss:
                    return STATUS_ACCEPT_LOSS;
                case StatusFlag.ChallengeMove:
                    return STATUS_CHALLENGE_MOVE;
                case StatusFlag.ChallengeWin:
                    return STATUS_CHALLENGE_WIN;
                case StatusFlag.DrawMove:
                    return STATUS_DRAW_MOVE;
                case StatusFlag.WinningMove:
                    return STATUS_WINNING_MOVE;
                case StatusFlag.AcceptDraw:
                    return STATUS_ACCEPT_DRAW;
                default:
                    return null;
            }
        }
        [DataContract]
        private class ServerRequestConfig
        {
            [DataMember]
            public string Url { get; set; }
            [DataMember]
            public string RequestData { get; set; }
            [DataMember]
            public int GameId { get; set; }
            [DataMember]
            public int MatchId { get; set; }
            [IgnoreDataMember]
            public Action<string,int> ResponseAction { get; set; }
        }

        #region Central Server Response Handling
        static void ChallengePlayerCompleted(string data, int matchId)
        {
            var response = JsonSerializer.DeseriaizeFromJSON<ChallengeResponse>(data);
            if (string.IsNullOrEmpty(response.Error))
            {
                Match match;
                CentralServerSession session;
                using (IGameDataService dataService = new GameDataService())
                {
                    match = dataService.GetMatch(matchId, null);
                    session = dataService.GetCentralServerSession(null, null, match.CurrentGameId.Value);

                    dataService.Attach(session);
                    session.CentralServerGameId = response.GameId;
                    dataService.Save();

                    TicTacToeHost.Instance.AcceptChallenge(match.PlayerTwoId, match.MatchId);

                    if (response.YourTurn)
                    {
                        dataService.SetPlayerTurn(match.CurrentGameId.Value, match.PlayerOneId);
                        dataService.Save();
                    }
                    else
                    {
                        dataService.SetPlayerTurn(match.CurrentGameId.Value, match.PlayerTwoId);
                        dataService.Save();
                        TicTacToeHost.Instance.Move(new Move() { GameId = match.CurrentGameId.Value, PlayerId = match.PlayerTwoId, X = response.X, Y = response.Y }, false);
                    }
                }
            }
        }

        static void PostMoveCompleted(string data, int matchId)
        {
            var response = JsonSerializer.DeseriaizeFromJSON<MoveResponse>(data);
            bool newGame = false;
            int gameId;
            int oldGameId;
            Match match;
            Player tttdPlayer;
            CentralServerSession session;
            StatusFlag flag = CentralServerCommunicationChannel.ParseStatus(response.StatusFlag);
            using (IGameDataService dataService = new GameDataService())
            {
                match = dataService.GetMatch(matchId, null);
                session = dataService.GetCentralServerSession(null, null, match.CurrentGameId.Value);
                tttdPlayer = dataService.GetPlayer(match.PlayerOneId);
                oldGameId = match.CurrentGameId.Value;

                if (response.NewGameId != null && response.NewGameId > 0 && response.NewGameId != session.CentralServerGameId)
                {
                    int newGameId = TicTacToeHost.Instance.ConfigureGame(matchId);
                    gameId = newGameId;
                    newGame = true;
                    session = dataService.CreateCentralServerSession(newGameId, response.NewGameId);
                }
                else
                {
                    gameId = match.CurrentGameId.Value;
                }
                dataService.Save();
            }

            if (response.YourTurn
                //|| (newGame && response.X == null && response.Y == null)
                )
            {
                using (IGameDataService dataService = new GameDataService())
                {
                    dataService.SetPlayerTurn(gameId, match.PlayerOneId);
                    dataService.Save();
                }
            }
            else if(flag == StatusFlag.ChallengeMove || flag == StatusFlag.ChallengeWin)
            {
                using (IGameDataService dataService = new GameDataService())
                {
                    dataService.EndGame(oldGameId, null);
                    dataService.Save();
                }
            }
            else if (response.X >= 0 && response.Y >= 0)
            {
                if (flag == StatusFlag.AcceptLoss)
                {
                    using (IGameDataService dataService = new GameDataService())
                    {
                        dataService.EndGame(oldGameId, match.PlayerOneId);
                        dataService.Save();
                    }
                }

                if (newGame)
                {
                    using (IGameDataService dataService = new GameDataService())
                    {
                        dataService.SetPlayerTurn(gameId, match.PlayerTwoId);
                        dataService.Save();
                    }
                }

                Move move = new Move() { GameId = gameId, PlayerId = match.PlayerTwoId };
                GameState state = TicTacToeHost.Instance.GetGameState(gameId, match.PlayerTwoId);
                if (state.Mode == PlayMode.DeathMatch)
                {
                    move.OriginX = response.X;
                    move.OriginY = response.Y;
                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            if (state.GameBoard[x][y] == null || state.GameBoard[x][y] == 0)
                            {
                                move.X = x;
                                move.Y = y;
                            }
                        }
                    }
                }
                else
                {
                    move.X = response.X.Value;
                    move.Y = response.Y.Value;
                }

                MoveResult opponentMoveResult = TicTacToeHost.Instance.Move(move, false);
                GameState postMoveState = TicTacToeHost.Instance.GetGameState(gameId, match.PlayerTwoId);
                if (opponentMoveResult != MoveResult.Valid || flag == StatusFlag.WinningMove || postMoveState.YouWon)
                {
                    MoveRequest challengeRequest = new MoveRequest();
                    challengeRequest.GameId = session.CentralServerGameId.Value;
                    challengeRequest.PlayerName = tttdPlayer.PlayerName;
                    challengeRequest.X = 0;
                    challengeRequest.Y = 0;

                    bool challenging = false;
                    if (opponentMoveResult != MoveResult.Valid)
                    {
                        challengeRequest.Flags = CentralServerCommunicationChannel.GetStatus(StatusFlag.ChallengeMove);
                        challenging = true;
                    }
                    else
                    {
                        if (flag == StatusFlag.WinningMove && !postMoveState.YouWon)
                        {
                            challengeRequest.Flags = CentralServerCommunicationChannel.GetStatus(StatusFlag.ChallengeWin);
                            challenging = true;
                        }
                        else
                        {
                            challengeRequest.Flags = CentralServerCommunicationChannel.GetStatus(StatusFlag.AcceptLoss);
                        }
                    }

                    if (challenging)
                    {
                        using (IGameDataService dataService = new GameDataService())
                        {
                            dataService.EndGame(gameId, null);
                            dataService.Save();
                        }
                    }

                    CentralServerCommunicationChannel.Instance.PostMove(challengeRequest, match.CurrentGameId.Value, match.MatchId);
                }
            }
        }
        #endregion

        #region Tic Tac Toe Event Handling
        public void Instance_PlayerMove(object sender, MoveEventArgs e)
        {
            (this as ICommunicationChannel).PostMove(e.MatchId, e.OriginX != null ? e.OriginX.Value : e.X, e.OriginY != null ? e.OriginY.Value : e.Y, e.Mode);
        }

        public void Instance_PlayerChallenge(object sender, ChallengeEventArgs e)
        {
            (this as ICommunicationChannel).ChallengePlayer(e.MatchId);
        }
        #endregion

        #region Disposable
        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                }

                this.disposed = true;
            }
        }
        #endregion
    }
}
