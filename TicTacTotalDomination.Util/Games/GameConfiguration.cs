using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TicTacTotalDomination.Util.Games
{
    public enum PlayerType { Human, AI }
    public enum GameType { Local, Network }

    public class GameConfiguration
    {
        public GameConfiguration.Player PlayerOne { get; set; }
        public GameConfiguration.Player PlayerTwo { get; set; }

        public GameType GameType { get; set; }
        public int MatchRounds { get; set; }

        public class Player
        {
            public PlayerType PlayerType { get; set; }
            public string Name { get; set; }
        }
    }
}
