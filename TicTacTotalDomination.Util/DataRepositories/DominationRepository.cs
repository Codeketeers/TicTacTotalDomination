using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TicTacTotalDomination.Util.Models;

namespace TicTacTotalDomination.Util.DataRepositories
{
    public class DominationRepository : IDominationRepository
    {
        private bool disposed;
        public TicTacTotalDominationContext Context { get; set; }

        public DominationRepository(string connectionString)
        {
            this.disposed = false;
            this.Context = new TicTacTotalDominationContext(connectionString);
        }

        IQueryable<Match> IDominationRepository.GetMatches()
        {
            return this.Context.Matches.AsNoTracking();
        }

        IQueryable<Game> IDominationRepository.GetGames()
        {
            return this.Context.Games.AsNoTracking();
        }

        IQueryable<GameMove> IDominationRepository.GetGameMoves()
        {
            return this.Context.GameMoves.AsNoTracking();
        }

        IQueryable<Player> IDominationRepository.GetPlayers()
        {
            return this.Context.Players.AsNoTracking();
        }

        IQueryable<CentralServerSession> IDominationRepository.GetCentralServerSessions()
        {
            return this.Context.CentralServerSessions.AsNoTracking();
        }

        IQueryable<AIGame> IDominationRepository.GetAIGames()
        {
            return this.Context.AIGames.AsNoTracking();
        }

        IQueryable<AIAttentionRequiredResult> IDominationRepository.GetAIGamesRequiringAttention()
        {
            return this.Context.GetAIGamesRequiringAttention();
        }

        IQueryable<ConfigSection> IDominationRepository.GetConfigSections()
        {
            return this.Context.ConfigSections.AsNoTracking();
        }

        IQueryable<AuditLog> IDominationRepository.GetAuditLogs()
        {
            return this.Context.AuditLogs.AsNoTracking();
        }

        IQueryable<AuditLogSection> IDominationRepository.GetAuditLogSections()
        {
            return this.Context.AuditLogSections.AsNoTracking();
        }

        IQueryable<AuditLog> IDominationRepository.GetAllAuditLogsForMatch(int matchId)
        {
            return this.Context.GetAllAuditLogsForMatch(matchId);
        }

        Match IDominationRepository.CreateMatch()
        {
            var result = new Match();
            this.Context.Entry(result).State = System.Data.Entity.EntityState.Added;
            return result;
        }

        Game IDominationRepository.CreateGame()
        {
            var result = new Game();
            this.Context.Entry(result).State = System.Data.Entity.EntityState.Added;
            return result;
        }

        GameMove IDominationRepository.CreateGameMove()
        {
            var result = new GameMove();
            this.Context.Entry(result).State = System.Data.Entity.EntityState.Added;
            return result;
        }

        Player IDominationRepository.CreatePlayer()
        {
            var result = new Player();
            this.Context.Entry(result).State = System.Data.Entity.EntityState.Added;
            return result;
        }

        CentralServerSession IDominationRepository.CreateCentralServerSession()
        {
            var result = new CentralServerSession();
            this.Context.Entry(result).State = System.Data.Entity.EntityState.Added;
            return result;
        }

        AIGame IDominationRepository.CreateAIGame()
        {
            var result = new AIGame();
            this.Context.Entry(result).State = System.Data.Entity.EntityState.Added;
            return result;
        }

        ConfigSection IDominationRepository.CreateConfigSection()
        {
            var result = new ConfigSection();
            this.Context.Entry(result).State = System.Data.Entity.EntityState.Added;
            return result;
        }

        AuditLog IDominationRepository.CreateAuditLog()
        {
            var result = new AuditLog();
            this.Context.Entry(result).State = System.Data.Entity.EntityState.Added;
            return result;
        }

        AuditLogSection IDominationRepository.CreateAuditLogSection()
        {
            var result = new AuditLogSection();
            this.Context.Entry(result).State = System.Data.Entity.EntityState.Added;
            return result;
        }

        void IDominationRepository.Attach<T>(T entity)
        {
            this.Context.Entry(entity).State = System.Data.Entity.EntityState.Modified;
        }

        void IDominationRepository.Delete(object entity)
        {
            this.Context.Entry(entity).State = System.Data.Entity.EntityState.Deleted;
        }

        void IDominationRepository.Save()
        {
            this.Context.SaveChanges();
        }

        #region Disposable
        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }
        
        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.Context.Dispose();
                }

                this.disposed = true;
            }
        }
        #endregion
    }
}
