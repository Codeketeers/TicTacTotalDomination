﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TicTacTotalDomination.Util.Models;

namespace TicTacTotalDomination.Util.DataServices
{
    public interface IGameDataService : IDisposable
    {
        Player GetOrCreatePlayer(string playerName);
        Game CreateGame(Player playerOne, Player playerTwo, Models.Match match);
        Game GetGame(int gameId);
        Match CreateMatch(Player playerOne, Player playerTwo);
        Match GetMatch(int? matchId, int? gameId);
        AIGame CreateAIGame(Player player, Game game, Match match);
        AIGame GetAIGame(int gameId);
        CentralServerSession CreateCentralServerSession(int gameId);
        CentralServerSession GetCentralServerSession(int? sessionId, int? centralServerGameId, int? gameId);
        IEnumerable<GameMove> GetGameMoves(int gameId);
        IEnumerable<AIAttentionRequiredResult> GetAIGamesRequiringAttention();
        void Move(int gameId, int playerId, int? origX, int? origY, int x, int y);
        void SetPlayerTurn(int gameId, int playerId);
        void SwapPlayerTurn(int gameId);

        void Attach(object entity);
        void Delete(object entity);
        void Save();
    }
}
