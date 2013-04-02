using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TicTacTotalDomination.Util.Models;

namespace TicTacTotalDomination.Util.DataServices
{
    public interface IGameDataService : IDisposable
    {
        Player GetOrCreatePlayer(string playerName);
        Game CreateGame(Player playerOne, Player playerTwo);
        Game GetGame(int gameId);
        IEnumerable<GameMove> GetGameMoves(int gameId);
        void Move(int gameId, int playerId, int? origX, int? origY, int x, int y);
    }
}
