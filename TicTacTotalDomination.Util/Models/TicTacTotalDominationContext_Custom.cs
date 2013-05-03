using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        public IQueryable<AuditLog> GetAllAuditLogsForMatch(int matchId)
        {
            SqlParameter matchParam = new SqlParameter(){ ParameterName = "matchId", Value = matchId};

            return base.Database.SqlQuery<AuditLog>("EXEC [dbo].[sp_GetAllLogsForMatch] @matchId", matchParam).AsQueryable();
        }
    }

    public class AIAttentionRequiredResult
    {
        public int GameId { get; set; }
        public int PlayerId { get; set; }
    }
}
