using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TicTacTotalDomination.Util.Games;
using TicTacTotalDomination.Util.Models;
using TicTacTotalDomination.Web.Sessions;

namespace TicTacTotalDomination.Web.Controllers
{
    public class GameController : ApiController
    {
        private TicTacToeHost host;

        public GameController()
        {
            this.host = TicTacToeHost.Instance;
        }

        [HttpGet]
        public Notification GetNotification(int playerId, int? matchId)
        {
            return this.host.GetNotification(playerId, matchId);
        }

        [HttpPost]
        public Player SignIn(string playerName)
        {
            return this.host.SignInPlayer(playerName);
        }

        [HttpPost]
        public int StartMatch(GameConfiguration config)
        {
            return this.host.InitiateChallenge(config);
        }

        [HttpGet]
        public bool GetHasGameStateChanged(int gameId, string stateDateString)
        {
            return this.host.IsGameStateChaged(gameId, stateDateString);
        }

        [HttpGet]
        public GameState GetGameState(int gameId, int playerId)
        {
            return this.host.GetGameState(gameId, playerId);
        }

        [HttpGet]
        public GameState GetGameStateIfChanged(int gameId, int playerId, string stateDateString)
        {
            GameState result = null;
            if (this.host.IsGameStateChaged(gameId, stateDateString))
            {
                result = this.host.GetGameState(gameId, playerId);
            }
            return result;
        }

        [HttpGet]
        public bool GetHasMatchStateChanged(int matchId, string stateDateString)
        {
            return this.host.IsMatchStateChanged(matchId, stateDateString);
        }

        [HttpGet]
        public void AcceptMatch(int playerId, int matchId)
        {
            this.host.AcceptChallenge(playerId, matchId);
        }

        [HttpGet]
        public void DeclineMatch(int playerId, int matchId)
        {
            this.host.DeclineChallenge(playerId, matchId);
        }

        [HttpGet]
        public MatchState GetMatchState(int matchId, int playerId)
        {
            return this.host.GetMatchState(matchId, playerId);
        }

        [HttpPost]
        public MoveResult Move(Move move)
        {
            return this.host.Move(move);
        }
    }
}
