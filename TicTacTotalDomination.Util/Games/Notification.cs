using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TicTacTotalDomination.Util.Games
{
    public class Notification
    {
        public List<NotificationGame> Notifications { get; set; }

        public class NotificationGame
        {
            public int GameId { get; set; }
            public bool NewGame { get; set; }
            public bool MyTurn { get; set; }
            public string OpponentName { get; set; }
        }
    }
}
