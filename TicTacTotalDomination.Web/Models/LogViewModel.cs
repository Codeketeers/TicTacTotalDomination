using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TicTacTotalDomination.Util.DataServices;
using TicTacTotalDomination.Util.Models;

namespace TicTacTotalDomination.Web.Models
{
    public class LogViewModel
    {
        public List<LogViewModel.Log> Logs = new List<Log>();
        public LogViewModel()
        {
            this.Logs = new List<Log>();
        }
        public LogViewModel(int matchId)
            : this()
        {
            using (IGameDataService dataService = new GameDataService())
            {
                Match match = dataService.GetMatch(matchId, null);
                Player playerOne = dataService.GetPlayer(match.PlayerOneId);
                Player playerTwo = dataService.GetPlayer(match.PlayerTwoId);
                IEnumerable<Game> games = dataService.GetGamesForMatch(matchId);
                IEnumerable<AuditLog> auditLogs = dataService.GetAllAuditLogsForMatch(matchId);

                LogViewModel.Log matchStartLogEntry = new Log();
                matchStartLogEntry.LogType = "MatchStart";
                matchStartLogEntry.LogDateTime = match.CreateDate;
                matchStartLogEntry.Metadata = string.Format("{0} vs. {1}", playerOne.PlayerName, playerTwo.PlayerName);
                matchStartLogEntry.Message = null;
                this.Logs.Add(matchStartLogEntry);

                if (match.EndDate != null)
                {
                    LogViewModel.Log matchEndLogEntry = new Log();
                    matchEndLogEntry.LogType = match.WinningPlayerId != null ? "MatchWon" : "MatchEnded";
                    matchEndLogEntry.LogDateTime = match.EndDate.Value;
                    if (match.WinningPlayerId != null)
                    {
                        Player winningPlayer = dataService.GetPlayer(match.WinningPlayerId.Value);
                        matchEndLogEntry.Metadata = string.Format("Winner: {0}", winningPlayer.PlayerName);
                    }
                    else
                        matchEndLogEntry.Metadata = "Match ended in error.";

                    this.Logs.Add(matchEndLogEntry);
                }

                foreach (var log in auditLogs)
                {
                    string message = string.Join("", dataService.GetAuditLogSections(log.LogId).OrderBy(section => section.SectionId).Select(section => section.Section));
                    LogViewModel.Log logEntry = new Log();
                    logEntry.LogType = log.LogType;
                    logEntry.LogDateTime = log.LogDateTime;
                    logEntry.Metadata = log.Metadata;
                    logEntry.Message = message;

                    this.Logs.Add(logEntry);
                }

                foreach (var game in games)
                {
                    LogViewModel.Log gameStartLogEntry = new Log();
                    gameStartLogEntry.LogType = "GameStart";
                    gameStartLogEntry.LogDateTime = game.CreateDate;
                    gameStartLogEntry.Metadata = string.Format("Game Id: {0}", game.GameId);
                    gameStartLogEntry.Message = null;
                    this.Logs.Add(gameStartLogEntry);

                    if (game.EndDate != null)
                    {
                        LogViewModel.Log gameEndLogEntry = new Log();
                        gameEndLogEntry.LogType = game.WinningPlayerId != null ? "GameWon" : "GameEnded";
                        gameEndLogEntry.LogDateTime = game.EndDate.Value;
                        if (game.WinningPlayerId != null)
                        {
                            Player winningPlayer = dataService.GetPlayer(game.WinningPlayerId.Value);
                            gameEndLogEntry.Metadata = string.Format("Winner: {0}", winningPlayer.PlayerName);
                        }
                        else
                            gameEndLogEntry.Metadata = "Game ended in error.";

                        this.Logs.Add(gameEndLogEntry);
                    }

                    var moves = dataService.GetGameMoves(game.GameId).GroupBy(mv => new { mv.PlayerId, mv.MoveDate });
                    foreach (var move in moves)
                    {
                        LogViewModel.Log moveLogEntry = new Log();
                        Player movePlayer = dataService.GetPlayer(move.First().PlayerId);

                        moveLogEntry.LogType = "DatabaseMove";
                        moveLogEntry.LogDateTime = move.First().MoveDate;
                        moveLogEntry.Metadata = movePlayer.PlayerName;

                        string message;
                        if (move.Count() >= 2)
                        {
                            GameMove unsetMove = move.FirstOrDefault(uMove => !uMove.IsSettingPiece);
                            GameMove setMove = move.FirstOrDefault(sMove => sMove.IsSettingPiece);

                            message = string.Format("OriginX: {0}, OriginY: {1}, X: {2}, Y: {3}", unsetMove != null ? (int?)unsetMove.X : null
                                                                                                , unsetMove != null ? (int?)unsetMove.y : null
                                                                                                , setMove != null ? (int?)setMove.X : null
                                                                                                , setMove != null ? (int?)setMove.y : null);
                        }
                        else
                        {
                            GameMove setMove = move.First();
                            message = string.Format("X: {0}, Y: {1}", setMove.X, setMove.y);
                        }
                        moveLogEntry.Message = message;
                        this.Logs.Add(moveLogEntry);
                    }
                }
            }
        }

        public class Log
        {
            public string LogType { get; set; }
            public DateTime LogDateTime { get; set; }
            public string Metadata { get; set; }
            public string Message { get; set; }
        }
    }
}