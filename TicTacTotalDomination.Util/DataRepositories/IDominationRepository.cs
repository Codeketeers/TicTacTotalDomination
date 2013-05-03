using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TicTacTotalDomination.Util.Models;

namespace TicTacTotalDomination.Util.DataRepositories
{
    public interface IDominationRepository : IDisposable
    {
        IQueryable<Match> GetMatches();
        IQueryable<Game> GetGames();
        IQueryable<GameMove> GetGameMoves();
        IQueryable<Player> GetPlayers();
        IQueryable<CentralServerSession> GetCentralServerSessions();
        IQueryable<AIGame> GetAIGames();
        IQueryable<AIAttentionRequiredResult> GetAIGamesRequiringAttention();
        IQueryable<ConfigSection> GetConfigSections();
        IQueryable<AuditLog> GetAuditLogs();
        IQueryable<AuditLogSection> GetAuditLogSections();
        IQueryable<AuditLog> GetAllAuditLogsForMatch(int matchId);

        Match CreateMatch();
        Game CreateGame();
        GameMove CreateGameMove();
        Player CreatePlayer();
        CentralServerSession CreateCentralServerSession();
        AIGame CreateAIGame();
        ConfigSection CreateConfigSection();
        AuditLog CreateAuditLog();
        AuditLogSection CreateAuditLogSection();

        void Attach<T>(T entity) where T : class;
        void Delete(object entity);
        void Save();
    }
}
