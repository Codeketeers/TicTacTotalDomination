using System;
using System.Collections.Generic;

namespace TicTacTotalDomination.Util.Models
{
    public partial class AuditLogSection
    {
        public int SectionId { get; set; }
        public int AuditLogId { get; set; }
        public string Section { get; set; }
        public virtual AuditLog AuditLog { get; set; }
    }
}
