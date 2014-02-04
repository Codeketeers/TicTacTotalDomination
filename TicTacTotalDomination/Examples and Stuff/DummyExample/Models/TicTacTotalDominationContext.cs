using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace DummyExample.Models
{
    public class TicTacTotalDominationContext
    {
        private static int Id = 1;

        //The prefered method I usually use for singletons.
        private static Lazy<TicTacTotalDominationContext> _Instance = new Lazy<TicTacTotalDominationContext>(() => new TicTacTotalDominationContext());
        public static TicTacTotalDominationContext Instance
        {
            get { return _Instance.Value; }
        }
        private TicTacTotalDominationContext() { }

        public Dictionary<Guid, Game> TicTacToeGames
        {
            get
            {
                if (HttpRuntime.Cache["TicTacToeGames"] == null)
                    HttpRuntime.Cache.Insert("TicTacToeGames", new Dictionary<Guid, Game>(), null, DateTime.Now.AddDays(1), Cache.NoSlidingExpiration);

                return (Dictionary<Guid, Game>)HttpRuntime.Cache["TicTacToeGames"];
            }
        }

        public Game GetGame(Guid identifier)
        {
            return this.TicTacToeGames[identifier];
        }

        public Guid CreateGame(CreateGameOptions options)
        {
            Guid identifier = Guid.NewGuid();
            Game game = new Game();

            game.Id = Id++;
            game.PlayerOneName = options.PlayerOneName;
            game.PlayerTwoName = options.PlayerTwoName;
            game.Board = new char[3][];
            for (int i = 0; i < 3; i++)
            {
                game.Board[i] = new char[3];
            }

            game.Board[0][0] = 'X';
            game.Board[2][2] = 'Y'; //Just to show something on the board.

            this.TicTacToeGames.Add(identifier, game);

            return identifier;
        }
    }

    public class Game
    {
        public int Id { get; set; }
        public string PlayerOneName { get; set; }
        public string PlayerTwoName { get; set; }
        public string WinningPlayer { get; set; }
        public char[][] Board { get; set; }
    }
}