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

        Match CreateMatch();
        Game CreateGame();
        GameMove CreateGameMove();
        Player CreatePlayer();
        CentralServerSession CreateCentralServerSession();

        void Attach(object entity);
        void Delete(object entity);
        void Save();
    }
}
