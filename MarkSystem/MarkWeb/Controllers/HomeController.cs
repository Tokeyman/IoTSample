using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MarkDbModel;
using MarkDbModel.Entity;

namespace MarkWeb.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db { get; set; }

        public HomeController() => db = DbSession.GetDbContext();

        public ActionResult Index()
        {
            return View(db.OnlineCache.ToList());
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