using DummyExample.Models;
using DummyExample.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DummyExample.Controllers
{
    //Note that every controller in this project ends its name in Controller
    //  and also inherits from the Controller class.
    //When creating views to bind to, they will go in the views folder,
    //  under the appropriately named folder.
    //In this case there is a folder in Views called Game.
    public class GameController : Controller
    {
        //MVC will attempt to create an instance of the parameters in this method,
        //  and will also try to map values in the GET/POST to propeties of those
        //  instances.
        public ActionResult StartGame(CreateGameOptions options)
        {
            GameViewModel model = new GameViewModel();
            model.CreateGame(options);

            //The string "GameBoard" corresponds to the name of a view in the Game folder.
            //Also, we use PartialView() here so we only render the html that's in the view,
            //  and not use the full layout page.
            return PartialView("GameBoard", model);
        }

        public ActionResult Move(Guid gameId, int x, int y, char piece)
        {
            GameViewModel model = new GameViewModel(gameId);
            model.PerformMove(x, y, piece);

            //I'll go ahead and just re-render the view.
            //  We could leverage the client side by just returning
            //      JSON data that represents the game, and use 
            //      javascript on the client to update the screen.
            return PartialView("GameBoard", model);
        }
    }
}
