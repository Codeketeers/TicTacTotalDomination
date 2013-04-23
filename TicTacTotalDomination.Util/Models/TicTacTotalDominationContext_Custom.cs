using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TicTacTotalDomination.Util.Models
{
    public partial class TicTacTotalDominationContext
    {
        public TicTacTotalDominationContext(string connectionString)
            : base(connectionString) { }

        public IQueryable<AIAttentionRequiredResult> GetAIGamesRequiringAttention()
        {
            return base.Database.SqlQuery<AIAttentionRequiredResult>("execute dbo.sp_GetAIGamesForEvaluation").AsQueryable();
        }
    }

    public class AIAttentionRequiredResult
    {
        public int GameId { get; set; }
        public int PlayerId { get; set; }
    }
}
