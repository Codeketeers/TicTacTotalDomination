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
            CurrentGamesViewModel model = new CurrentGamesViewModel(SessionManager.Instance.IsPlayerLoggedIn ? (int?)SessionManager.Instance.PlayerId : null);
            return View(model);
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
        public ActionResult SignOut()
        {
            SessionManager.Instance.ClearSession();

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
            return View("PlayGame");
        }

        [HttpGet]
        public ActionResult PlayGame(int matchId, string playerName)
        {
            if (!SessionManager.Instance.IsPlayerLoggedIn || SessionManager.Instance.PlayerName != playerName)
            {
                GameController gameApiController = new GameController();
                Player player = gameApiController.SignIn(playerName);
                SessionManager.Instance.PlayerName = playerName;
                SessionManager.Instance.PlayerId = player.PlayerId;
                SessionManager.Instance.IsPlayerLoggedIn = true;
            }

            SessionManager.Instance.MatchId = matchId;

            return View();
        }

        [HttpGet]
        public ActionResult History()
        {
            HistoryViewModel model = new HistoryViewModel(SessionManager.Instance.IsPlayerLoggedIn ? (int?)SessionManager.Instance.PlayerId : null);
            return View(model);
        }

        [HttpGet]
        public ActionResult Log(int matchId)
        {
            LogViewModel model = new LogViewModel(matchId);
            return View(model);
        }
    }
}
