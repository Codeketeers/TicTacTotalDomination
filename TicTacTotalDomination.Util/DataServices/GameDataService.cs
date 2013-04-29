using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using TicTacTotalDomination.Util.DataRepositories;
using TicTacTotalDomination.Util.Models;

namespace TicTacTotalDomination.Util.DataServices
{
    public class GameDataService : IGameDataService
    {
        private bool disposed = false;
        private IDominationRepository repository;

        public IDominationRepository Repository { get { return this.repository; } }

        public GameDataService()
        {
            string connecitonString = ConfigurationManager.ConnectionStrings["TicTacTotalDomination"].ConnectionString;
            this.repository = new DominationRepository(connecitonString);
        }

        Models.Player IGameDataService.GetOrCreatePlayer(string playerName)
        {
            Player result = this.repository.GetPlayers().FirstOrDefault(player => player.PlayerName == playerName);
            //If the result is null, that means the player doesn't exist.
            if (result == null)
            {
                result = this.repository.CreatePlayer();
                result.PlayerName = playerName;
                this.repository.Save();
            }

            return result;
        }

        Player IGameDataService.GetPlayer(int playerId)
        {
            return this.repository.GetPlayers().FirstOrDefault(player => player.PlayerId == playerId);
        }

        Models.Game IGameDataService.CreateGame(Models.Player playerOne, Models.Player playerTwo, Models.Match match)
        {
            Game result = this.repository.CreateGame();
            result.MatchId = match.MatchId;
            result.PlayerOneId = playerOne.PlayerId;
            result.PlayerTwoId = playerTwo.PlayerId;
            result.CreateDate = DateTime.Now;
            result.StateDate = result.CreateDate;
            this.repository.Save();

            return result;
        }

        Models.Game IGameDataService.GetGame(int gameId)
        {
            return this.repository.GetGames().FirstOrDefault(game => game.GameId == gameId);
        }

        Models.Match IGameDataService.CreateMatch(Models.Player playerOne, Models.Player playerTwo)
        {
            Match result = this.repository.CreateMatch();
            result.PlayerOneId = playerOne.PlayerId;
            result.PlayerTwoId = playerTwo.PlayerId;
            result.CreateDate = DateTime.Now;
            this.repository.Save();

            return result;
        }

        Models.Match IGameDataService.GetMatch(int? matchId, int? gameId)
        {
            //We can find a match with either its id or with the id of a game in the match
            Match result = null;
            if (matchId != null)
            {
                result = this.repository.GetMatches().FirstOrDefault(match => match.MatchId == matchId);
            }
            else if (gameId != null)
            {
                Game matchGame = this.repository.GetGames().FirstOrDefault(game => game.GameId == gameId);
                if (matchGame != null && matchGame.MatchId != null)
                {
                    result = this.repository.GetMatches().FirstOrDefault(match => match.MatchId == matchGame.MatchId);
                }
            }
            return result;
        }

        AIGame IGameDataService.CreateAIGame(Player player, Game game, Match match)
        {
            AIGame result = this.repository.CreateAIGame();
            result.PlayerId = player.PlayerId;
            result.GameId = game.GameId;

            if (match != null)
                result.MatchId = match.MatchId;

            return result;
        }

        AIGame IGameDataService.GetAIGame(int gameId, int playerId)
        {
            return this.repository.GetAIGames().FirstOrDefault(aiGame => aiGame.GameId == gameId && aiGame.PlayerId == playerId);
        }

        Models.CentralServerSession IGameDataService.CreateCentralServerSession(int gameId)
        {
            CentralServerSession result = this.repository.CreateCentralServerSession();
            result.GameId = gameId;
            this.repository.Save();

            return result;
        }

        Models.CentralServerSession IGameDataService.GetCentralServerSession(int? sessionId, int? centralServerGameId, int? gameId)
        {
            CentralServerSession result = null;
            if (sessionId != null)
            {
                result = this.repository.GetCentralServerSessions().FirstOrDefault(session => session.CentralServerSessionId == sessionId.Value);
            }
            else if (centralServerGameId != null)
            {
                result = this.repository.GetCentralServerSessions().FirstOrDefault(session => session.CentralServerGameId == centralServerGameId);
            }
            else if (gameId != null)
            {
                result = this.repository.GetCentralServerSessions().FirstOrDefault(session => session.GameId == gameId.Value);
            }
            return result;
        }

        ConfigSection IGameDataService.CreateConfigSection(int matchId, string contents)
        {
            ConfigSection result = this.repository.CreateConfigSection();
            result.MatchId = matchId;
            result.Section = contents;

            return result;
        }

        IEnumerable<ConfigSection> IGameDataService.GetConfigSections(int matchId)
        {
            return this.repository.GetConfigSections().Where(section => section.MatchId == matchId).ToList();
        }

        IEnumerable<Game> IGameDataService.GetGamesForPlayer(int playerId)
        {
            return this.repository.GetGames().Where(game => game.PlayerOneId == playerId || game.PlayerTwoId == playerId).ToList();
        }

        IEnumerable<Match> IGameDataService.GetPendingMatchesForPlayer(int playerId)
        {
            return this.repository.GetMatches().Where(game => ((game.PlayerOneId == playerId && game.PlayerOneAccepted == null) 
                                                                || (game.PlayerTwoId == playerId && game.PlayerTwoAccepted == null))
                                                            && game.EndDate == null).ToList();
        }

        IEnumerable<Models.GameMove> IGameDataService.GetGameMoves(int gameId)
        {
            return this.repository.GetGameMoves().Where(move => move.GameId == gameId).ToList();
        }

        IEnumerable<AIAttentionRequiredResult> IGameDataService.GetAIGamesRequiringAttention()
        {
            return this.repository.GetAIGamesRequiringAttention().ToList();
        }

        void IGameDataService.Move(int gameId, int playerId, int? origX, int? origY, int x, int y)
        {
            Game game = (this as IGameDataService).GetGame(gameId);
            if (game != null && (game.PlayerOneId == playerId || game.PlayerTwoId == playerId))
            {
                DateTime moveDateTime = DateTime.Now;
                if (origX != null && origY != null)
                {
                    GameMove originUnset = this.repository.CreateGameMove();
                    originUnset.GameId = gameId;
                    originUnset.IsSettingPiece = false;
                    originUnset.MoveDate = moveDateTime;
                    originUnset.PlayerId = playerId;
                    originUnset.X = origX.Value;
                    originUnset.y = origY.Value;
                }
                
                GameMove move = this.repository.CreateGameMove();
                move.GameId = gameId;
                move.IsSettingPiece = true;
                move.MoveDate = moveDateTime;
                move.PlayerId = playerId;
                move.X = x;
                move.y = y;

                this.repository.Attach(game);
                game.StateDate = moveDateTime;
                //if (game.CurrentPlayerId == game.PlayerOneId)
                //    game.CurrentPlayerId = game.PlayerTwoId;
                //else if (game.CurrentPlayerId == game.PlayerTwoId)
                //    game.CurrentPlayerId = game.PlayerOneId;

                this.repository.Save();
            }
        }

        void IGameDataService.SetPlayerTurn(int gameId, int playerId)
        {
            Game game = (this as IGameDataService).GetGame(gameId);
            if (game != null && (game.PlayerOneId == playerId || game.PlayerTwoId == playerId))
            {
                this.repository.Attach(game);
                game.CurrentPlayerId = playerId;
                game.StateDate = DateTime.Now;
            }
        }

        void IGameDataService.SwapPlayerTurn(int gameId)
        {
            Game game = (this as IGameDataService).GetGame(gameId);
            if (game != null)
            {
                this.repository.Attach(game);
                if (game.CurrentPlayerId == game.PlayerOneId)
                    game.CurrentPlayerId = game.PlayerTwoId;
                else if (game.CurrentPlayerId == game.PlayerTwoId)
                    game.CurrentPlayerId = game.PlayerOneId;
                //Otherwise, there is no current player, so we have to way of knowing how to swap. Do nothing.
            }
        }

        void IGameDataService.EndGame(int gameId, int? winningPlayer)
        {
            Game game = (this as IGameDataService).GetGame(gameId);
            Match match = (this as IGameDataService).GetMatch(null, gameId);
            if (game != null)
            {
                DateTime endDate = DateTime.Now;
                this.repository.Attach(game);
                this.repository.Attach(match);
                game.EndDate = endDate;
                match.StateDate = endDate;

                if (winningPlayer != null && (winningPlayer == game.PlayerOneId || winningPlayer == game.PlayerTwoId))
                {
                    game.WinningPlayerId = winningPlayer;
                    game.WonDate = endDate;
                    game.StateDate = endDate;
                }
            }
        }

        void IGameDataService.Attach(object entity)
        {
            this.repository.Attach(entity);
        }
        void IGameDataService.Delete(object entity)
        {
            this.repository.Delete(entity);
        }
        void IGameDataService.Save()
        {
            this.repository.Save();
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
                    this.repository.Dispose();
                }

                this.disposed = true;
            }
        }
        #endregion
    }
}
