using System;
using System.Collections.Generic;

namespace TicTacTotalDomination.Util.Models
{
    public partial class ConfigSection
    {
        public int SectionId { get; set; }
        public int MatchId { get; set; }
        public string Section { get; set; }
        public virtual Match Match { get; set; }
    }
}
