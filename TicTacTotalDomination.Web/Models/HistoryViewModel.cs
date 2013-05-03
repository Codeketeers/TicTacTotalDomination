using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TicTacTotalDomination.Util.DataServices;
using DB = TicTacTotalDomination.Util.Models;

namespace TicTacTotalDomination.Web.Models
{
    public class HistoryViewModel
    {
        public List<History> Histories { get; set; }

        public HistoryViewModel()
        {
            this.Histories = new List<History>();
        }
        public HistoryViewModel(int? playerId)
            : this()
        {
            if (playerId != null)
            {
                using (IGameDataService dataService = new GameDataService())
                {
                    IEnumerable<DB.Match> playingMatches = dataService.GetAllMatchesForPlayer(playerId.Value);
                    foreach (var match in playingMatches)
                    {
                        DB.Player opponent = dataService.GetPlayer(match.PlayerOneId == playerId ? match.PlayerTwoId : match.PlayerOneId);
                        DB.Game currentGame = dataService.GetGame(match.CurrentGameId.Value);
                        DB.Player winningPlayer = null;
                        if (match.WinningPlayerId != null)
                            winningPlayer = dataService.GetPlayer(match.WinningPlayerId.Value);

                        HistoryViewModel.History history = new History();
                        history.MatchId = match.MatchId;
                        history.OpponentName = opponent.PlayerName;
                        history.Winner = winningPlayer != null ? winningPlayer.PlayerName : "none";
                        history.EndDate = match.EndDate;
                        history.StartDateTime = match.CreateDate;

                        this.Histories.Add(history);
                    }
                }
            }
        }

        public class History
        {
            public string OpponentName { get; set; }
            public DateTime StartDateTime { get; set; }
            public DateTime? EndDate { get; set; }
            public string Winner { get; set; }
            public int MatchId { get; set; }
        }
    }
}