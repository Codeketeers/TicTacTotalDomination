using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TicTacTotalDomination.Util.DataServices;
using DB = TicTacTotalDomination.Util.Models;

namespace TicTacTotalDomination.Web.Models
{
    public class CurrentGamesViewModel
    {
        public List<CurrentGamesViewModel.Game> Games { get; set; }

        public CurrentGamesViewModel()
        {
            this.Games = new List<Game>();
        }
        public CurrentGamesViewModel(int? playerId)
            :this()
        {
            if (playerId != null)
            {
                using (IGameDataService dataService = new GameDataService())
                {
                    IEnumerable<DB.Match> playingMatches = dataService.GetPlayingMatchesForPlayer(playerId.Value);
                    foreach (var match in playingMatches)
                    {
                        DB.Player opponent = dataService.GetPlayer(match.PlayerOneId == playerId ? match.PlayerTwoId : match.PlayerOneId);
                        DB.Game currentGame = dataService.GetGame(match.CurrentGameId.Value);
                        DB.Player currentPlayer = null;
                        if (currentGame.CurrentPlayerId != null)
                            currentPlayer = dataService.GetPlayer(currentGame.CurrentPlayerId.Value);

                        CurrentGamesViewModel.Game game = new Game();
                        game.MatchId = match.MatchId;
                        game.OpponentName = opponent.PlayerName;
                        game.PlayerTurn = currentPlayer != null ? currentPlayer.PlayerName : "none";
                        game.StartDateTime = match.CreateDate;

                        this.Games.Add(game);
                    }
                }
            }
        }

        public class Game
        {
            public string OpponentName { get; set; }
            public DateTime StartDateTime { get; set; }
            public string PlayerTurn { get; set; }
            public int MatchId { get; set; }
        }
    }
}