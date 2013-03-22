using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TicTacTotalDomination.Util.Models;

namespace TicTacTotalDomination.Util.DataRepositories
{
    public interface IDominationRepository : IDisposable
    {
        IQueryable<Game> GetGames();
        IQueryable<GameMove> GetGameMoves();
        IQueryable<Player> GetPlayers();

        Game CreateGame();
        GameMove CreateGameMove();
        Player CreatePlayer();

        void Attach(object entity);
        void Delete(object entity);
        void Save();
    }
}
