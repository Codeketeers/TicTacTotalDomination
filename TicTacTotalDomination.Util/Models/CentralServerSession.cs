using System;
using System.Collections.Generic;

namespace TicTacTotalDomination.Util.Models
{
    public partial class CentralServerSession
    {
        public int CentralServerSessionId { get; set; }
        public Nullable<int> CentralServerGameId { get; set; }
        public int GameId { get; set; }
        public virtual Game Game { get; set; }
    }
}
