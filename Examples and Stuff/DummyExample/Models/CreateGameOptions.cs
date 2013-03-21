using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DummyExample.Models
{
    public enum GameModes{Local, Network}

    public class CreateGameOptions
    {
        //If you want properties of your models/view models to bind
        //  , they need to be public properties like this.
        //Fields like string PlayerName; wont work.
        public string PlayerOneName { get; set; }
        public string PlayerTwoName { get; set; }

        //MVC model binding can bind just about anything.
        //Including enum values.
        public GameModes PlayerMode { get; set; }

        public int NumberOfPlayers { get; set; }
    }
}