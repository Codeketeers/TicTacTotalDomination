using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using TicTacTotalDomination.Util.DataServices;
using TicTacTotalDomination.Util.Games;
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
            }
        }

        void ICommunicationChannel.PostMove(int matchId, int x, int y)
        {
            using (IGameDataService dataService = new GameDataService())
            {
                Match match = dataService.GetMatch(matchId, null);
                Player player = dataService.GetPlayer(match.PlayerOneId);

                var requestData = new MoveRequest();
                requestData.PlayerName = player.PlayerName;
                requestData.GameId = -1;
                requestData.X = x;
                requestData.Y = y;

                CentralServerSession session = dataService.GetCentralServerSession(null, null, match.CurrentGameId.Value);
                if (session != null)
                    requestData.GameId = session.CentralServerGameId.Value;

                string requestJSON = JsonSerializer.SerializeToJSON<MoveRequest>(requestData);
                var requestConfig = new ServerRequestConfig();
                requestConfig.Url = string.Format("{0}/play.php", ConfigurationManager.AppSettings["CentralServerUrl"]);
                requestConfig.RequestData = requestJSON;
                requestConfig.GameId = match.CurrentGameId.Value;
                requestConfig.ResponseAction = new Action<string, int>(PostMoveCompleted);
            }
        }

        private void PerformServerRequest(ServerRequestConfig config)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, args) =>
                {
                    var workerConfig = (ServerRequestConfig)args.Argument;

                    //TODO: Add configuration value for url.
                    var request = (HttpWebRequest)WebRequest.Create(workerConfig.Url);
                    request.Method = WebRequestMethods.Http.Post;

                    using(Stream requestStream = request.GetRequestStream())
                    using (var requestWriter = new StreamWriter(requestStream))
                    {
                        requestWriter.Write(workerConfig);
                        requestWriter.Flush();
                        requestWriter.Close();
                    }

                    string responseData = null;
                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (Stream responseStream = response.GetResponseStream())
                    using (var responseReader = new StreamReader(responseStream))
                    {
                        responseData = responseReader.ReadToEnd();
                    }

                    workerConfig.ResponseAction(responseData, workerConfig.GameId);
                };
            worker.RunWorkerAsync(config);
        }

        private static StatusFlag ParseStatus(string status)
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
                default:
                    return StatusFlag.None;
            }
        }

        private static string GetStatus(StatusFlag status)
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
                default:
                    return null;
            }
        }

        private class ServerRequestConfig
        {
            public string Url { get; set; }
            public string RequestData { get; set; }
            public int GameId { get; set; }
            public int MatchId { get; set; }
            public Action<string,int> ResponseAction { get; set; }
        }

        #region Central Server Response Handling
        static void ChallengePlayerCompleted(string data, int gameId)
        {
            var response = JsonSerializer.DeseriaizeFromJSON<ChallengeResponse>(data);
            if (string.IsNullOrEmpty(response.Error))
            {
                using (IGameDataService dataService = new GameDataService())
                {
                    var game = dataService.GetGame(gameId);
                    var session = dataService.GetCentralServerSession(null, null, gameId);
                    session.CentralServerGameId = response.GameId;
                    //I'll need some of the other game logic in order to finish this call.
                }
            }
        }

        static void PostMoveCompleted(string data, int gameId)
        {
            //var response = JsonSerializer.DeseriaizeFromJSON<MoveResponse>(data);
            //if (string.IsNullOrEmpty(response.Error))
            //{
            //    using (IGameDataService dataService = new GameDataService())
            //    {
            //        Game game = dataService.GetGame(gameId);
            //        GameState state = TicTacToeHost.Instance.GetGameState(gameId, game.PlayerTwoId);
            //        StatusFlag status = CentralServerCommunicationChannel.ParseStatus(response.StatusFlag);
            //        Move move = new Move();

            //        move.GameId = gameId;
            //        if(status == StatusFlag.DrawMove)
            //        {
            //            move.OriginX = response.X;
            //            move.OriginY = response.Y;

            //            state.
            //        }
            //        else if(status == StatusFlag.None || status == StatusFlag.WinningMove)
            //        {
            //            move.X = response.X;
            //            move.Y = response.Y;
            //        }
            //        TicTacToeHost.Instance.Move()
            //    }
            //}
        }
        #endregion

        #region Tic Tac Toe Event Handling
        public void Instance_PlayerMove(object sender, MoveEventArgs e)
        {
            (this as ICommunicationChannel).PostMove(e.MatchId, e.OriginX != null ? e.OriginX.Value : e.X, e.OriginY != null ? e.OriginY.Value : e.Y);
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
