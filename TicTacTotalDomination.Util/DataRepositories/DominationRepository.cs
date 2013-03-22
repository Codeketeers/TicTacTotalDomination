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

        Game IDominationRepository.CreateGame()
        {
            Game result = new Game();
            this.Context.Entry(result).State = System.Data.Entity.EntityState.Added;
            return result;
        }

        GameMove IDominationRepository.CreateGameMove()
        {
            GameMove result = new GameMove();
            this.Context.Entry(result).State = System.Data.Entity.EntityState.Added;
            return result;
        }

        Player IDominationRepository.CreatePlayer()
        {
            Player result = new Player();
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
