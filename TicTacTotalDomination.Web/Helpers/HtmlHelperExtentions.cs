using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TicTacTotalDomination.Web.Helpers
{
    public static class HtmlHelperExtentions
    {
        public static void SetActivePage(this HtmlHelper helper, string page)
        {
            helper.ViewBag.Page = page;
        }
    }
}