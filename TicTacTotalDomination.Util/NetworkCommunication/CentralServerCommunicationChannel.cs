﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
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

                this.PerformServerRequest(requestConfig);
            }
        }

        void ICommunicationChannel.PostMove(int matchId, int x, int y)
        {
            using (IGameDataService dataService = new GameDataService())
            {
                Match match = dataService.GetMatch(matchId, null);
                Player player = dataService.GetPlayer(match.PlayerOneId);
                GameState state = TicTacToeHost.Instance.GetGameState(match.CurrentGameId.Value, player.PlayerId);

                var requestData = new MoveRequest();
                requestData.PlayerName = player.PlayerName;
                requestData.GameId = -1;
                requestData.Flags = CentralServerCommunicationChannel.GetStatus(state.Mode == PlayMode.DeathMatch ? StatusFlag.DrawMove : state.Mode == PlayMode.Won ? StatusFlag.WinningMove : StatusFlag.None);
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
                requestConfig.MatchId = matchId;
                requestConfig.ResponseAction = new Action<string, int>(PostMoveCompleted);

                this.PerformServerRequest(requestConfig);
            }
        }

        private void PerformServerRequest(ServerRequestConfig config)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, args) =>
                {
                    var workerConfig = (ServerRequestConfig)args.Argument;

                    //"http://centralserver.codeketeers.com/ServerPairing.php?challenge=James&from=Anthony"
                    JavaScriptSerializer dataSerailizer = new JavaScriptSerializer();
                    string queryString = string.Join("&", dataSerailizer.Deserialize<Dictionary<string,string>>(config.RequestData).Where(kv => !string.IsNullOrEmpty(kv.Value)).Select(kv => string.Format("{0}={1}", kv.Key, HttpUtility.UrlEncode(kv.Value != null ? kv.Value : string.Empty))));
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format("{0}?{1}", config.Url, queryString));
                    request.ContentLength = 0;

                    var httpResponse = (HttpWebResponse)request.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        workerConfig.ResponseAction(result, workerConfig.MatchId);
                    }
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
            bool newGame;
            int gameId;
            Match match;
            CentralServerSession session;
            using (IGameDataService dataService = new GameDataService())
            {
                match = dataService.GetMatch(matchId, null);
                session = dataService.GetCentralServerSession(null, null, match.CurrentGameId.Value);

                if (response.NewGameId != null && response.NewGameId > 0 && response.NewGameId != session.CentralServerGameId)
                {
                    int newGameId = TicTacToeHost.Instance.ConfigureGame(matchId);
                    gameId = newGameId;
                    session = dataService.CreateCentralServerSession(newGameId, response.NewGameId);
                }
                else
                {
                    gameId = match.CurrentGameId.Value;
                }
                dataService.Save();
            }

            if (response.X >= 0 && response.Y >= 0)
            {
                Move move = new Move() { GameId = gameId, PlayerId = match.PlayerTwoId };
                GameState state = TicTacToeHost.Instance.GetGameState(gameId, match.PlayerTwoId);
                StatusFlag flag = CentralServerCommunicationChannel.ParseStatus(response.StatusFlag);
                if (flag == StatusFlag.DrawMove)
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
                    move.X = response.X;
                    move.Y = response.Y;
                }

                TicTacToeHost.Instance.Move(move, false);
            }
            else if (response.YourTurn)
            {
                using (IGameDataService dataService = new GameDataService())
                {
                    dataService.SetPlayerTurn(gameId, match.PlayerOneId);
                    dataService.Save();
                }
            }

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
