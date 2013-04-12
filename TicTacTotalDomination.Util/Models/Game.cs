using System;
using System.Collections.Generic;

namespace TicTacTotalDomination.Util.Models
{
    public partial class Game
    {
        public Game()
        {
            this.CentralServerSessions = new List<CentralServerSession>();
            this.GameMoves = new List<GameMove>();
        }

        public int GameId { get; set; }
        public Nullable<int> MatchId { get; set; }
        public int PlayerOneId { get; set; }
        public int PlayerTwoId { get; set; }
        public Nullable<int> WinningPlayerId { get; set; }
        public Nullable<int> CurrentPlayerId { get; set; }
        public System.DateTime CreateDate { get; set; }
        public Nullable<System.DateTime> WonDate { get; set; }
        public virtual ICollection<CentralServerSession> CentralServerSessions { get; set; }
        public virtual Player Player { get; set; }
        public virtual Match Match { get; set; }
        public virtual Player Player1 { get; set; }
        public virtual Player Player2 { get; set; }
        public virtual Player Player3 { get; set; }
        public virtual ICollection<GameMove> GameMoves { get; set; }
    }
}
