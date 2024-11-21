using DummyExample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DummyExample.ViewModels
{
    //View models are what you bind your views to.
    //They frequently will also contain alot of the application
    //  logic of your web app.
    public class GameViewModel
    {
        /*
         * Properties like this are picked up and compiled into something you're
         *      used to seeing in Java:
         *      
         * private Guid _GameId;
         * public void SetGameId(Guid value){this._GameId = value;}
         * public Guid GetGameId(){return this._GameId;}
         * 
         * They're just meant as a time saver, and are very powerfull tools.
         */
        public Guid GameId { get; set; }

        //Anything you want your view to bind to needs to be a public property.
        //Private properties or fields won't bind properly.
        public Game Game { get; set; }

        //If you overload the contructor, you need to make sure there is always
        //  a parameterless contructor if you want to use it for any kind of
        //  binding.
        public GameViewModel() { }
        public GameViewModel(Guid gameId)
        {
            this.GameId = gameId;
            this.Game = TicTacTotalDominationContext.Instance.GetGame(gameId);
        }

        public void CreateGame(CreateGameOptions options)
        {
            this.GameId = TicTacTotalDominationContext.Instance.CreateGame(options);
            this.Game = TicTacTotalDominationContext.Instance.GetGame(this.GameId);
        }

        public void PerformMove(int x, int y, char piece)
        {
            this.Game.Board[x][y] = piece;
        }
    }
}