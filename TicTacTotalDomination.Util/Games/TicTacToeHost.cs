using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TicTacTotalDomination.Util.Caching;
using TicTacTotalDomination.Util.DataServices;
using TicTacTotalDomination.Util.Logging;
using TicTacTotalDomination.Util.Models;
using TicTacTotalDomination.Util.NetworkCommunication;
using TicTacTotalDomination.Util.Serialization;

namespace TicTacTotalDomination.Util.Games
{
    public class ChallengeEventArgs : EventArgs
    {
        public int PlayerOneId { get; set; }
        public int PlayerTwoId { get; set; }
        public int MatchId { get; set; }
    }

    public class MoveEventArgs : EventArgs
    {
        public int MatchId { get; set; }
        public int PlayerId { get; set; }
        public PlayMode Mode { get; set; }
        public int? OriginX { get; set; }
        public int? OriginY { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public delegate void ChallengeEventHandler(object sender, ChallengeEventArgs e);
    public delegate void MoveEventHandler(object sender, MoveEventArgs e);

    public class TicTacToeHost
    {
        private static Lazy<TicTacToeHost> _Instance = new Lazy<TicTacToeHost>(() => new TicTacToeHost());
        public static TicTacToeHost Instance { get { return _Instance.Value; } }
        private TicTacToeHost() { }

        public event ChallengeEventHandler PlayerChallenge;
        public event MoveEventHandler PlayerMove;

        public Player SignInPlayer(string playerName)
        {
            Player result;
            using (IGameDataService gameDataService = new GameDataService())
            {
                result = gameDataService.GetOrCreatePlayer(playerName);
            }
            Logger.Instance.Log("PlayerSignIn", null, playerName);

            return result;
        }

        public Notification GetNotification(int playerId, int? matchId)
        {
            Notification result = new Notification() { Notifications = new List<Notification.NotificationMatch>() };

            using (IGameDataService gameDataService = new GameDataService())
            {
                IEnumerable<Match> matches = gameDataService.GetPendingMatchesForPlayer(playerId);

                foreach (var match in matches)
                {
                    Player opponent = gameDataService.GetPlayer(match.PlayerOneId == playerId ? match.PlayerTwoId : match.PlayerOneId);
                    result.Notifications.Add(new Notification.NotificationMatch()
                                            {
                                                OpponentName = opponent.PlayerName,
                                                MatchId = match.MatchId
                                            });
                }
            }

            return result;
        }

        public int InitiateChallenge(GameConfiguration config)
        {
            using (IGameDataService gameDataService = new GameDataService())
            {
                Models.Player playerOne = gameDataService.GetOrCreatePlayer(config.PlayerOne.Name);
                Models.Player playerTwo = gameDataService.GetOrCreatePlayer(config.PlayerTwo.Name);
                Models.Match match = gameDataService.CreateMatch(playerOne, playerTwo);
                match.StateDate = DateTime.Now;
                match.NumberOfRounds = config.MatchRounds;

                GameConfigCache.Instance.CacheConfig(match.MatchId, config);
                string serializedConfig = JsonSerializer.SerializeToJSON(config);
                IEnumerable<string> configSections = StringSplitter.SplitString(serializedConfig, 500);
                List<ConfigSection> dbConfigSections = new List<ConfigSection>();
                foreach(var section in configSections)
                {
                    dbConfigSections.Add(gameDataService.CreateConfigSection(match.MatchId, section));
                }

                if (config.PlayerTwo.PlayerType == PlayerType.AI && config.GameType == GameType.Local)
                    match.PlayerTwoAccepted = true;

                gameDataService.Save();

                int gameId = ConfigureGame(match.MatchId);

                //Contact the central server
                //In this case, we are just handling data validation, and game rules.
                //We will let the server response tell us who goes first.
                if (config.GameType == GameType.Network)
                {
                    this.PlayerChallenge(this, new ChallengeEventArgs()
                    {
                        MatchId = match.MatchId,
                        PlayerOneId = playerOne.PlayerId,
                        PlayerTwoId = playerTwo.PlayerId
                    });
                }

                return match.MatchId;
            }
        }

        public void AcceptChallenge(int playerId, int matchId)
        {
            UpdateChallengeState(playerId, matchId, true);
        }

        public void DeclineChallenge(int playerId, int matchId)
        {
            UpdateChallengeState(playerId, matchId, false);
        }

        private void UpdateChallengeState(int playerId, int matchId, bool? state)
        {
            using (IGameDataService gameDataService = new GameDataService())
            {
                Match match = gameDataService.GetMatch(matchId, null);
                GameConfiguration config = GameConfigCache.Instance.GetConfig(matchId);

                if (match.EndDate == null)
                {
                    gameDataService.Attach(match);

                    bool challengeStateChange = false;
                    if (match.PlayerOneId == playerId && match.PlayerOneAccepted != state)
                    {
                        match.PlayerOneAccepted = state;
                        challengeStateChange = true;
                    }
                    else if (match.PlayerTwoId == playerId && match.PlayerTwoAccepted != state)
                    {
                        match.PlayerTwoAccepted = state;
                        challengeStateChange = true;
                    }

                    if (playerId == match.PlayerOneId || playerId == match.PlayerTwoId && challengeStateChange)
                    {
                        match.StateDate = DateTime.Now;
                        if (match.PlayerOneAccepted == false && match.PlayerTwoAccepted == false)
                        {
                            match.EndDate = match.StateDate;
                        }
                        else if (match.PlayerOneAccepted == true 
                            && match.PlayerTwoAccepted == true
                            && config.GameType != GameType.Network)
                        {
                            gameDataService.SetPlayerTurn(match.CurrentGameId.Value, match.PlayerOneId);
                        }
                    }

                    gameDataService.Save();
                }
            }
        }

        public void CancelChallenge(int matchId, string reason)
        {
            using (IGameDataService gameDataService = new GameDataService())
            {
                Match match = gameDataService.GetMatch(matchId, null);
                gameDataService.Attach(match);
                match.EndDate = DateTime.Now;
                match.StateDate = match.EndDate.Value;

                List<Game> matchGames = gameDataService.Repository.GetGames().Where(game => game.MatchId == matchId).ToList();
                foreach (var game in matchGames)
                {
                    if (game.EndDate == null)
                    {
                        gameDataService.Attach(game);
                        game.EndDate = match.EndDate;
                        game.StateDate = match.EndDate.Value;
                    }
                }

                gameDataService.Save();
            }
            Logger.Instance.Log("MatchCancelled", string.Format("match:{0}", matchId), reason);
        }

        ///// <summary>
        ///// Set up a new game between two players. Will return the id of the created game.
        ///// </summary>
        ///// <param name="config">The parameters necessary to set up the game.</param>
        ///// <returns></returns>
        //public int ConfigureGame(GameConfiguration config)
        //{
        //    return ConfigureGame(config, null, true, true);
        //}

        public int ConfigureGame(int matchId)
        {
            using (IGameDataService gameDataService = new GameDataService())
            {
                //Create a game, as well as a match in case the players play multiple games in a row.
                Models.Match match = gameDataService.GetMatch(matchId, null);

                if (match == null)
                    throw new InvalidOperationException("A match is required for game play.");

                Models.Player playerOne = gameDataService.GetPlayer(match.PlayerOneId);
                Models.Player playerTwo = gameDataService.GetPlayer(match.PlayerTwoId);

                Models.Game game = gameDataService.CreateGame(playerOne, playerTwo, match);
                GameConfiguration config = GameConfigCache.Instance.GetConfig(matchId);

                //Make an entry in the table for AI to track the game.
                Models.AIGame aiPlayerOne;
                if (config.PlayerOne.PlayerType == PlayerType.AI)
                    aiPlayerOne = gameDataService.CreateAIGame(playerOne, game, match);

                //We only want to be responsible for managing local AIs.
                //If it's networked, don't record it.
                Models.AIGame aiPlayerTwo;
                if (config.PlayerTwo.PlayerType == PlayerType.AI && config.GameType != GameType.Network)
                    aiPlayerTwo = gameDataService.CreateAIGame(playerTwo, game, match);


                gameDataService.Attach(match);
                match.CurrentGameId = game.GameId;
                match.StateDate = game.StateDate;

                gameDataService.Save();

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
                if(game != null)
                    return game.StateDate.ToString("yyyyMMddHHmmssfffff") != stateDateString;
                return false;
            }
        }

        public bool IsMatchStateChanged(int matchId, string stateDateString)
        {
            using (IGameDataService gameDataService = new GameDataService())
            {
                Match match = gameDataService.GetMatch(matchId, null);
                if (match != null)
                    return match.StateDate.ToString("yyyyMMddHHmmssfffff") != stateDateString;
                else
                    return false;
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
                            result.GameBoard[move.X][move.y] = move.PlayerId == playerId ? 1 : -1;
                        else
                            result.GameBoard[move.X][move.y] = null;
                    }

                    if (result.GameBoard.Sum(row => row.Count(cell => cell == null || cell == 0)) <= 1 && result.Mode == PlayMode.Playing)
                    {
                        if (game.DeathMatchMode)
                            result.Mode = PlayMode.DeathMatch;
                    }

                    result.StateDateString = game.StateDate.ToString("yyyyMMddHHmmssfffff");
                    result.YourTurn = game.CurrentPlayerId == playerId;

                    if (game.WinningPlayerId != null)
                    {
                        Player winner = gameDataService.GetPlayer(game.WinningPlayerId.Value);
                        result.YouWon = playerId == game.WinningPlayerId;
                        result.WinningPlayerName = winner.PlayerName;
                    }
                }
            }
            return result;
        }

        public MatchState GetMatchState(int matchId, int playerId)
        {
            MatchState result = null;

            using (IGameDataService gameDataServcie = new GameDataService())
            {
                Match match = gameDataServcie.GetMatch(matchId, null);

                if (match != null)
                {
                    result = new MatchState();
                    result.MatchId = matchId;
                    result.PlayerId = playerId;
                    result.CurrentGameId = match.CurrentGameId;

                    if ((match.PlayerOneAccepted == null || match.PlayerTwoAccepted == null) && match.WonDate == null && match.EndDate == null)
                        result.Mode = PlayMode.None;
                    else if (match.WonDate == null && match.EndDate == null)
                        result.Mode = PlayMode.Playing;
                    else if (match.WonDate != null)
                        result.Mode = PlayMode.Won;
                    else
                        result.Mode = PlayMode.Ended;

                    result.StateDateString = match.StateDate.ToString("yyyyMMddHHmmssfffff");

                    if (match.WinningPlayerId != null)
                    {
                        Player winner = gameDataServcie.GetPlayer(match.WinningPlayerId.Value);
                        result.YouWon = playerId == match.WinningPlayerId;
                        result.WinningPlayerName = winner.PlayerName;
                    }
                }
            }

            return result;
        }

        public MoveResult Move(Move move)
        {
            return this.Move(move, true);
        }

        public MoveResult Move(Move move, bool raiseEvent)
        {
            MoveResult validationResult = this.ValidateMove(move);
            bool canMove = true;
            if (validationResult != MoveResult.Valid)
                canMove = false;

            Logger.Instance.Log("MoveAttempt", string.Format("GameId:{1}|PlayerId:{2}|ValidationResult:{0}", validationResult.ToString(), move.GameId, move.PlayerId), JsonSerializer.SerializeToJSON(move));

            if (!canMove)
                return validationResult;

            Match match;
            GameState currentState = TicTacToeHost.Instance.GetGameState(move.GameId, move.PlayerId);
            using (IGameDataService gameDataService = new GameDataService())
            {
                match = gameDataService.GetMatch(null, move.GameId);
                gameDataService.Move(move.GameId, move.PlayerId, move.OriginX, move.OriginY, move.X, move.Y);
                gameDataService.Save();
            }

            this.UpdateGame(move.GameId);

            if (validationResult == MoveResult.Valid && raiseEvent)
            {
                GameConfiguration config = GameConfigCache.Instance.GetConfig(match.MatchId);
                if (config.GameType == GameType.Network)
                    this.PlayerMove(this, new MoveEventArgs()
                    {
                        MatchId = match.MatchId,
                        PlayerId = move.PlayerId,
                        Mode = currentState.Mode,
                        OriginX = move.OriginX,
                        OriginY = move.OriginY,
                        X = move.X,
                        Y = move.Y
                    });
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
                        || (state.GameBoard[move.OriginX.Value][move.OriginY.Value] != 1))
                    {
                        return MoveResult.InvalidOrigin;
                    }
                }

                if ((move.X < 0 || move.X > 2)
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

        private void UpdateGame(int gameId)
        {
            using (IGameDataService gameDataService = new GameDataService())
            {
                Game game = gameDataService.GetGame(gameId);
                GameState state = this.GetGameState(gameId, game.PlayerOneId);
                int gameWon = this.IsWon(state.GameBoard);
                if (gameWon == 1 || gameWon == -1)
                {
                    if (gameWon == 1)
                        gameDataService.EndGame(gameId, game.PlayerOneId);
                    else if (gameWon == -1)
                        gameDataService.EndGame(gameId, game.PlayerTwoId);
                }

                if (!game.DeathMatchMode)
                {
                    int ninthCheckPlayer = game.CurrentPlayerId == game.PlayerOneId ? 1 : -1;
                    bool canWinWithNinth = false;
                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            if ((state.GameBoard[x][y] == null || state.GameBoard[x][y] == 0)
                                && this.CanWin(state.GameBoard, ninthCheckPlayer, x, y) == ninthCheckPlayer)
                            {
                                canWinWithNinth = true;
                                goto FoundEmpty;
                            }
                        }
                    }

                FoundEmpty:
                    if (!canWinWithNinth)
                    {
                        gameDataService.setToDeathMatch(game.GameId);
                    }
                }

                gameDataService.Save();
            }
            using (IGameDataService gameDataService = new GameDataService())
            {
                gameDataService.SwapPlayerTurn(gameId);
                gameDataService.Save();
            }
        }

        public int IsWon(int?[][]board)
        {
            int result = IsWon(board, 1);
            if (result == 0)
                result = IsWon(board, -1);

            return result;
        }

        public int IsWon(int?[][]Board, int player)
        {
            if (Board[0][0] == player)
            {
                if (Board[0][0] == Board[0][1] &&
                        Board[0][1] == Board[0][2])
                    return player;

                if (Board[0][0] == Board[1][0] &&
                        Board[1][0] == Board[2][0])
                    return player;
            }

            if (Board[2][2] == player)
            {
                if (Board[2][0] == Board[2][1] &&
                        Board[2][1] == Board[2][2])
                    return player;

                if (Board[0][2] == Board[1][2] &&
                        Board[1][2] == Board[2][2])
                    return player;
            }

            if (Board[1][1] == player)
            {
                if (Board[0][1] == Board[1][1] &&
                        Board[1][1] == Board[2][1])
                    return player;

                if (Board[1][0] == Board[1][1] &&
                        Board[1][1] == Board[1][2])
                    return player;

                if (Board[0][0] == Board[1][1] &&
                        Board[1][1] == Board[2][2])
                    return player;

                if (Board[0][2] == Board[1][1] &&
                        Board[1][1] == Board[2][0])
                    return player;
            }

            return 0;
        }

        /// <summary>
        /// Make a copy of the game board.  Then assume the player makes that move, and evaluate if they won.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player"></param>
        /// <param name="openX"></param>
        /// <param name="openY"></param>
        /// <returns></returns>
        public int CanWin(int?[][] board, int player, int openX, int openY)
        {
            int?[][] boardCopy = new int?[3][];
            for(int x = 0; x < 3; x++)
            {
                boardCopy[x] = new int?[3];
                for(int y = 0; y < 3; y++)
                {
                    boardCopy[x][y] = board[x][y];
                }
            }
            boardCopy[openX][openY] = player;

            return this.IsWon(boardCopy, player);
        }
    }
}
