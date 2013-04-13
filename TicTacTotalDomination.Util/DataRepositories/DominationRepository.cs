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

        void IDominationRepository.Attach(object entity)
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
