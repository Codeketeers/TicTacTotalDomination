using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TicTacTotalDomination.Util.DataRepositories;
using TicTacTotalDomination.Util.Models;

namespace TicTacTotalDomination.Util.DataServices
{
    public interface IGameDataService : IDisposable
    {
        IDominationRepository Repository { get; }

        Player GetOrCreatePlayer(string playerName);
        Player GetPlayer(int playerId);
        Game CreateGame(Player playerOne, Player playerTwo, Models.Match match);
        Game GetGame(int gameId);
        Match CreateMatch(Player playerOne, Player playerTwo);
        Match GetMatch(int? matchId, int? gameId);
        AIGame CreateAIGame(Player player, Game game, Match match);
        AIGame GetAIGame(int gameId, int playerId);
        CentralServerSession CreateCentralServerSession(int gameId);
        CentralServerSession GetCentralServerSession(int? sessionId, int? centralServerGameId, int? gameId);
        ConfigSection CreateConfigSection(int matchId, string contents);
        IEnumerable<ConfigSection> GetConfigSections(int matchId);
        IEnumerable<Game> GetGamesForPlayer(int playerId);
        IEnumerable<Match> GetPendingMatchesForPlayer(int playerId);
        IEnumerable<GameMove> GetGameMoves(int gameId);
        IEnumerable<AIAttentionRequiredResult> GetAIGamesRequiringAttention();
        void Move(int gameId, int playerId, int? origX, int? origY, int x, int y);
        void SetPlayerTurn(int gameId, int playerId);
        void SwapPlayerTurn(int gameId);
        void EndGame(int gameId, int? winningPlayer);

        void Attach(object entity);
        void Delete(object entity);
        void Save();
    }
}
