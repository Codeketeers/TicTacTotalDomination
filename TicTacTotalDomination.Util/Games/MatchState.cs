using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TicTacTotalDomination.Util.Games
{
    public class MatchState
    {
        public int MatchId { get; set; }
        public int PlayerId { get; set; }
        public int? CurrentGameId { get; set; }
        public PlayMode Mode { get; set; }
        public string StateDateString { get; set; }
        public bool YouWon { get; set; }
        public string WinningPlayerName { get; set; }
    }
}
