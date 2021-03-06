﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using TicTacTotalDomination.Util.Games;
using TicTacTotalDomination.Util.Models;

namespace TicTacTotalDomination.Controllers
{
    public class GameController : ApiController
    {
        private TicTacToeHost host;

        public GameController()
        {
            this.host = TicTacToeHost.Instance;
        }

        [HttpGet]
        public Notification GetNotification(int playerId, int? gameId)
        {
            return this.host.GetNotification(playerId, gameId);
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
        public bool GetHasMatchStateChanged(int matchId, string stateDateString)
        {
            return this.host.IsMatchStateChanged(matchId, stateDateString);
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
