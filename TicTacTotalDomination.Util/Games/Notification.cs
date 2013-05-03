using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TicTacTotalDomination.Util.Games
{
    public class Notification
    {
        public List<NotificationMatch> Notifications { get; set; }

        public class NotificationMatch
        {
            public int MatchId { get; set; }
            public string OpponentName { get; set; }
        }
    }
}
