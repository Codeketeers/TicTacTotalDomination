using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using TicTacTotalDomination.Util.AI;
using TicTacTotalDomination.Util.Caching;
using TicTacTotalDomination.Util.Games;
using TicTacTotalDomination.Util.NetworkCommunication;
using TicTacTotalDomination.Web.Caching;

namespace TicTacTotalDomination.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            GameConfigCache.Instance.ApplyCacheMechanism(new GameConfigWebCache());
            CentralServerCommunicationChannel.Instance.StartWatchingForGameEvents();
            AIManager.Instance.StartMonitoring();
        }

    }
}