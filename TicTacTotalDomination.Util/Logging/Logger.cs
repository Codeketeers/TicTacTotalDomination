using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TicTacTotalDomination.Util.DataServices;
using TicTacTotalDomination.Util.Models;
using TicTacTotalDomination.Util.Serialization;

namespace TicTacTotalDomination.Util.Logging
{
    public class Logger
    {
        private static Lazy<Logger> _Instance = new Lazy<Logger>(() => new Logger());
        public static Logger Instance { get { return _Instance.Value; } }
        private Logger() { }

        public void Log(string logType, string metadata, string message)
        {
            using (IGameDataService dataService = new GameDataService())
            {
                AuditLog log = dataService.CreateAuditLog(logType, metadata);
                dataService.Save();

                IEnumerable<string> messageSections = StringSplitter.SplitString(message, 500);
                List<AuditLogSection> dbConfigSections = new List<AuditLogSection>();
                foreach (var section in messageSections)
                {
                    dbConfigSections.Add(dataService.CreateAuditLogSection(log.LogId, section));
                }
                dataService.Save();
            }
        }
    }
}
