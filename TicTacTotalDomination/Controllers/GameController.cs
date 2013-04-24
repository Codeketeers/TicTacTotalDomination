using System;
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
            this.host = new TicTacToeHost();
        }

        [HttpPost]
        public Player SignIn(string playerName)
        {
            return this.host.SignInPlayer(playerName);
        }

        [HttpPost]
        public int ConfigureGame(GameConfiguration config)
        {
            return this.host.ConfigureGame(config);
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

        [HttpPost]
        public MoveResult Move(Move move)
        {
            return this.host.Move(move);
        }
    }
}
