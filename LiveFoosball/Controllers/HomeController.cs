using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LiveFoosball.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Score = GameData.Game.Current == null ? new GameData.Score { Blue = 0, Red = 0 } : GameData.Game.Current.Score;
            ViewBag.ApiKey = ConfigurationManager.AppSettings["XivelyAPIKey"];
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}