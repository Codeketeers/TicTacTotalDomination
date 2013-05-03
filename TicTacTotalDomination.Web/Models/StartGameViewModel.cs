using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TicTacTotalDomination.Util.Games;
using TicTacTotalDomination.Web.Sessions;

namespace TicTacTotalDomination.Web.Models
{
    public enum VersusOption { PvP, PvAI, AIvAI }

    public class StartGameViewModel
    {
        public GameConfiguration Config { get; set; }

        public List<SelectListItem> VersusOptions { get; set; }
        public List<SelectListItem> GameTypes { get; set; }

        public VersusOption SelectedVersus { get; set; }

        public StartGameViewModel()
        {
            this.Config = new GameConfiguration();
            this.Config.PlayerOne = new GameConfiguration.Player();
            this.Config.PlayerOne.Name = SessionManager.Instance.PlayerName;

            this.Config.PlayerTwo = new GameConfiguration.Player();

            this.VersusOptions = new List<SelectListItem>()
            {
                new SelectListItem(){ Text = "Player vs. Player", Value = VersusOption.PvP.ToString() },
                new SelectListItem(){ Text = "Player vs. AI", Value = VersusOption.PvAI.ToString() },
                new SelectListItem(){ Text = "AI vs. AI", Value = VersusOption.AIvAI.ToString() }
            };

            this.GameTypes = new List<SelectListItem>()
            {
                new SelectListItem(){ Text = "Local", Value = GameType.Local.ToString() },
                new SelectListItem(){ Text = "Network", Value = GameType.Network.ToString() }
            };
        }

        public void StartGame()
        {
            switch(this.SelectedVersus)
            {
                case VersusOption.PvP:
                    this.Config.PlayerOne.PlayerType = PlayerType.Human;
                    this.Config.PlayerTwo.PlayerType = PlayerType.Human;
                    break;
                case VersusOption.PvAI:
                    this.Config.PlayerOne.PlayerType = PlayerType.Human;
                    this.Config.PlayerTwo.PlayerType = PlayerType.AI;
                    break;
                case VersusOption.AIvAI:
                    this.Config.PlayerOne.PlayerType = PlayerType.AI;
                    this.Config.PlayerTwo.PlayerType = PlayerType.AI;
                    break;
            }

            this.Config.MatchRounds = 3;

            SessionManager.Instance.MatchId = TicTacToeHost.Instance.InitiateChallenge(this.Config);
        }
    }
}