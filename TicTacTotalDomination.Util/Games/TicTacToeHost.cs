using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TicTacTotalDomination.Util.DataServices;
using TicTacTotalDomination.Util.Models;
using TicTacTotalDomination.Util.NetworkCommunication;

namespace TicTacTotalDomination.Util.Games
{
    public class TicTacToeHost
    {
        //Probably don't need this to be singleton. There's no resources necessary to restrict.
        //private static Lazy<TicTacToeHost> _Instance = new Lazy<TicTacToeHost>(() => new TicTacToeHost());
        //public static TicTacToeHost Instance { get { return _Instance.Value; } }
        //private TicTacToeHost() { }

        public Notification GetNotification(int playerId, int? currentGameId)
        {
            Notification result = new Notification() { Notifications = new List<Notification.NotificationGame>() };

            using (IGameDataService gameDataService = new GameDataService())
            {
                IEnumerable<Game> allGames = gameDataService.GetGamesForPlayer(playerId);
                IEnumerable<Game> currentGames = allGames.Where(game => game.EndDate == null && (currentGameId != null ? game.GameId != currentGameId.Value : true));

                foreach (var game in currentGames)
                {
                    IEnumerable<GameMove> myMoves = gameDataService.GetGameMoves(game.GameId).Where(move => move.PlayerId == playerId);
                    if (myMoves.Any() || game.CurrentPlayerId == playerId)
                    {
                        Player opponent = gameDataService.GetPlayer(game.PlayerOneId == playerId ? game.PlayerTwoId : game.PlayerOneId);
                        result.Notifications.Add(new Notification.NotificationGame()
                                                {
                                                    GameId = game.GameId,
                                                    OpponentName = opponent.PlayerName,
                                                    MyTurn = game.CurrentPlayerId == playerId,
                                                    NewGame = !myMoves.Any()
                                                });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Set up a new game between two players. Will return the id of the created game.
        /// </summary>
        /// <param name="config">The parameters necessary to set up the game.</param>
        /// <returns></returns>
        public int ConfigureGame(GameConfiguration config)
        {
            using (IGameDataService gameDataService = new GameDataService())
            {
                Models.Player playerOne = gameDataService.GetOrCreatePlayer(config.PlayerOne.Name);
                Models.Player playerTwo = gameDataService.GetOrCreatePlayer(config.PlayerTwo.Name);

                //Create a game, as well as a match in case the players play multiple games in a row.
                Models.Match match = gameDataService.CreateMatch(playerOne, playerTwo);
                Models.Game game = gameDataService.CreateGame(playerOne, playerTwo, match);

                //Make an entry in the table for AI to track the game.
                Models.AIGame aiPlayerOne;
                if(config.PlayerOne.PlayerType == PlayerType.AI)
                    aiPlayerOne = gameDataService.CreateAIGame(playerOne, game, match);

                //We only want to be responsible for managing local AIs.
                //If it's networked, don't record it.
                Models.AIGame aiPlayerTwo;
                if(config.PlayerTwo.PlayerType == PlayerType.AI && config.GameType != GameType.Network)
                    aiPlayerTwo = gameDataService.CreateAIGame(playerTwo, game, match);

                gameDataService.Save();

                //Contact the central server
                //In this case, we are just handling data validation, and game rules.
                //We will let the server response tell us who goes first.
                if (config.GameType == GameType.Network)
                {
                    using (ICommunicationChannel serverCommunicationChannel = new CentralServerCommunicationChannel())
                    {
                        //Call the server, but don't wait for the response, we will deal with the response when it comes back.
                        serverCommunicationChannel.ChallengePlayer(playerOne.PlayerName, playerTwo.PlayerName, game.GameId);
                    }
                }
                //If the game isn't network mode, we default to making the creating player go first
                else
                {
                    gameDataService.SetPlayerTurn(game.GameId, playerOne.PlayerId);
                    gameDataService.Save();
                }

                return game.GameId;
            }
        }

        /// <summary>
        /// Checks if the game has been updated.
        /// </summary>
        /// <param name="gameId">The id of the game being played.</param>
        /// <param name="stateDateString">String format of the last known game state, in yyyyMMddHHmmss format.</param>
        /// <returns></returns>
        public bool IsGameStateChaged(int gameId, string stateDateString)
        {
            using (IGameDataService gameDataService = new GameDataService())
            {
                Game game = gameDataService.GetGame(gameId);
                return game.StateDate.ToString("yyyyMMddHHmmss") != stateDateString;
            }
        }

        public GameState GetGameState(int gameId, int playerId)
        {
            GameState result = null;
            using (IGameDataService gameDataService = new GameDataService())
            {
                Game game = gameDataService.GetGame(gameId);
                //The game exists, so we definately have a state to pass back.
                if (game != null)
                {
                    result = new GameState();
                    if (game.CurrentPlayerId == null && game.WonDate == null && game.EndDate == null)
                        result.Mode = PlayMode.None;
                    else if (game.WonDate == null && game.EndDate == null)
                        result.Mode = PlayMode.Playing;
                    else if (game.WonDate != null)
                        result.Mode = PlayMode.Won;
                    else
                        result.Mode = PlayMode.Ended;

                    result.GameBoard = new int?[3][]
                                        {
                                            new int?[3],
                                            new int?[3],
                                            new int?[3]
                                        };
                    IEnumerable<GameMove> moves = gameDataService.GetGameMoves(gameId).OrderBy(move => move.MoveDate).ToList();
                    foreach (var move in moves)
                    {
                        if (move.IsSettingPiece)
                            result.GameBoard[move.X][move.y] = move.PlayerId;
                        else
                            result.GameBoard[move.X][move.y] = null;
                    }

                    result.StateDateString = game.StateDate.ToString("yyyyMMddHHmmss");
                    result.YourTurn = game.CurrentPlayerId == playerId;
                }
            }
            return result;
        }

        public MoveResult Move(Move move)
        {
            MoveResult validationResult = this.ValidateMove(move);
            if (validationResult != MoveResult.Valid)
                return validationResult;

            using (IGameDataService gameDataService = new GameDataService())
            {
                gameDataService.Move(move.GameId, move.PlayerId, move.OriginX, move.OriginY, move.X, move.Y);
                gameDataService.SwapPlayerTurn(move.GameId);
                gameDataService.Save();
            }

            return validationResult;
        }

        public MoveResult ValidateMove(Move move)
        {
            GameState state = this.GetGameState(move.GameId, move.PlayerId);
            if (state == null)
                return MoveResult.InvalidGame;
            else if (state.YourTurn == false)
                return MoveResult.NotPlayerTurn;
            else
            {
                if (move.OriginX != null || move.OriginY != null)
                {
                    if ((state.Mode != PlayMode.DeathMatch)
                        || (move.OriginX == null || move.OriginY == null)
                        || (move.OriginX < 0 || move.OriginX > 2)
                        || (move.OriginY < 0 || move.OriginY > 2)
                        || (state.GameBoard[move.OriginX.Value][move.OriginY.Value] != move.PlayerId))
                    {
                        return MoveResult.InvalidOrigin;
                    }
                }

                if ((move.X == null || move.Y == null)
                    || (move.X < 0 || move.X > 2)
                    || (move.Y < 0 || move.Y > 2)
                    || (state.GameBoard[move.X][move.Y] != null))
                {
                    return MoveResult.InvalidDestination;
                }

                return MoveResult.Valid;
            }

            //If there isn't a rule to catch by now, we just need to fail.
            return MoveResult.Failed;
        }
    }
}
