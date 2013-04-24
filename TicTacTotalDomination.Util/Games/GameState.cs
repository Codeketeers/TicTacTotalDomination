using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TicTacTotalDomination.Util.Games
{
    public enum PlayMode { None, Playing, DeathMatch, Won, Ended }

    public class GameState
    {
        public PlayMode Mode { get; set; }
        /// <summary>
        /// Two dimensional array representing the game board.
        /// Populated with the id of the player occupying the space, if any.
        /// </summary>
        public int?[][] GameBoard { get; set; }
        public bool YourTurn { get; set; }
        public string StateDateString { get; set; }
    }
}
