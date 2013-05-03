using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TicTacTotalDomination.Util.Models;

namespace TicTacTotalDomination.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SignIn(string playerName)
        {
            GameController gameApiController = new GameController();
            Player player = gameApiController.SignIn(playerName);

            HttpContext.Session["playerName"] = player.PlayerName;
            HttpContext.Session["playerId"] = player.PlayerId;
            HttpContext.Session["loggedIn"] = true;

            return RedirectToAction("Index");
        }
    }
}
