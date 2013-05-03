using System;
using System.Collections.Generic;

namespace TicTacTotalDomination.Util.Models
{
    public partial class AIGame
    {
        public int AIGameId { get; set; }
        public Nullable<int> MatchId { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public bool EvaluatingMove { get; set; }
        public virtual Game Game { get; set; }
        public virtual Match Match { get; set; }
        public virtual Player Player { get; set; }
    }
}
