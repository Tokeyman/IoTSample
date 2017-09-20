using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MarkDbModel;
using MarkDbModel.Entity;
using System.Net;
using System.Threading.Tasks;

namespace MarkWeb.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db { get; set; }

        public HomeController() => db = DbSession.GetDbContext();

        public ActionResult Index()
        {
            return View(db.MarkClient.ToList());
        }

        [HttpPost]
        public async Task<ActionResult> CreateClient(string CreateGuid, string CreateDescription)
        {
            if (string.IsNullOrWhiteSpace(CreateGuid)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var m = db.MarkClient.FirstOrDefault(f => f.ClientGuid == CreateGuid);
            if (m == null)
            {
                var model = new MarkClient(CreateGuid, CreateDescription);
                db.MarkClient.Add(model);
                await db.SaveChangesAsync();
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditClient(string Id)
        {
            if (string.IsNullOrWhiteSpace(Id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = db.MarkClient.FirstOrDefault(f => f.Id == Id);
            if (model == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var CommandGroup = db.CommandGroup.OrderBy(o => o.Description).ToList();
            var CommandGroupList = Utility.GetSelectList(CommandGroup, "Id", "Description");
            ViewBag.CommandGroupList = Utility.GetSelected(CommandGroupList, model.CommandGroupId);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> EditClient(MarkClient model)
        {

            var m = db.MarkClient.FirstOrDefault(f => f.Id == model.Id);
            if (m != null)
            {
                m.ClientGuid = model.ClientGuid;
                m.Description = model.Description;
                m.CommandGroupId = model.CommandGroupId;
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> RemoveClient(string RemoveClientId)
        {
            if (string.IsNullOrWhiteSpace(RemoveClientId)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var m = db.MarkClient.FirstOrDefault(f => f.Id == RemoveClientId);
            if (m != null)
            {
                db.MarkClient.Remove(m);
                await db.SaveChangesAsync();
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
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