using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TicTacTotalDomination.Util.Models;
using TicTacTotalDomination.Web.Models;
using TicTacTotalDomination.Web.Sessions;

namespace TicTacTotalDomination.Web.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            //ViewBag.Page = "Home";
            return View();
        }

        [HttpPost]
        public ActionResult SignIn(string playerName)
        {
            //ViewBag.Page = "Home";
            GameController gameApiController = new GameController();
            Player player = gameApiController.SignIn(playerName);

            SessionManager.Instance.PlayerName = player.PlayerName;
            SessionManager.Instance.PlayerId =  player.PlayerId;
            SessionManager.Instance.IsPlayerLoggedIn = true;

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult StartGame()
        {
            //ViewBag.Page = "StartGame";
            return View(new StartGameViewModel());
        }

        [HttpPost]
        public ActionResult StartGame(StartGameViewModel model)
        {
            model.StartGame();
            return View("GamePlay");
        }
    }
}
