using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace TicTacTotalDomination.Web.Sessions
{
    public class SessionManager
    {
        private static Lazy<SessionManager> _Instance = new Lazy<SessionManager>(() => new SessionManager());
        public static SessionManager Instance { get { return _Instance.Value; } }
        private SessionManager() { }

        private HttpSessionState Session { get { return HttpContext.Current.Session; } }

        private void SetSessionValue(string key, object value)
        {
            this.Session[key] = value;
        }

        private T GetSessionValue<T>(string key)
        {
            if (this.Session[key] != null)
                return (T)this.Session[key];
            else
                return default(T);
        }

        public void ClearSession()
        {
            this.Session.Clear();
        }

        public bool IsPlayerLoggedIn
        {
            get { return this.GetSessionValue<bool>("playerLoggedIn"); }
            set { this.SetSessionValue("playerLoggedIn", value); }
        }

        public int PlayerId
        {
            get { return this.GetSessionValue<int>("playerId"); }
            set { this.SetSessionValue("playerId", value); }
        }

        public string PlayerName
        {
            get { return this.GetSessionValue<string>("playerName"); }
            set { this.SetSessionValue("playerName", value); }
        }

        public int? MatchId
        {
            get { return this.GetSessionValue<int?>("matchId"); }
            set { this.SetSessionValue("matchId", value); }
        }
    }
}