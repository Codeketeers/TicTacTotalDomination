using System;
using System.Collections.Generic;

namespace TicTacTotalDomination.Util.Models
{
    public partial class AuditLog
    {
        public AuditLog()
        {
            this.AuditLogSections = new List<AuditLogSection>();
        }

        public int LogId { get; set; }
        public string LogType { get; set; }
        public System.DateTime LogDateTime { get; set; }
        public string Metadata { get; set; }
        public virtual ICollection<AuditLogSection> AuditLogSections { get; set; }
    }
}
