using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using TicTacTotalDomination.Util.DataServices;
using TicTacTotalDomination.Util.Models;

namespace TicTacTotalDomination.Util.AI
{
    public class AIManager
    {
        private Timer aiTimer = null;

        private static Lazy<AIManager> _Instance = new Lazy<AIManager>(() => new AIManager());
        public static AIManager Instance { get { return _Instance.Value; } }
        private AIManager() 
        {
            this.aiTimer = new Timer();
            this.aiTimer.Elapsed += aiTimer_Elapsed;
            this.aiTimer.Interval = new TimeSpan(0, 0, 1).TotalMilliseconds;
        }

        public void StartMonitoring()
        {
            this.aiTimer.Start();
        }

        public void StopMonitoring()
        {
            this.aiTimer.Stop();
        }

        void aiTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            using(IGameDataService gameDataService = new GameDataService())
            {
                IEnumerable<AIAttentionRequiredResult> aiGames = gameDataService.GetAIGamesRequiringAttention();

                //If we have any Games that the AI needs to play, loop through them all.
                //We will notify the AI to play on seperate threads.
                if (aiGames.Any())
                {
                    foreach(var aiGame in aiGames)
                    {
                        BackgroundWorker aiWorker = new BackgroundWorker();
                        aiWorker.DoWork += aiWorker_DoWork;
                        aiWorker.RunWorkerAsync(aiGame);
                    }
                }
            }
        }

        void aiWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            AIAttentionRequiredResult aiGameAttentionRequired = (AIAttentionRequiredResult)e.Argument;
            //TODO:
            //This is where we need to get game state, verify the move is require, and notify the AI.
        }
    }
}
