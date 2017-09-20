using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using MarkDbModel;
using MarkDbModel.Entity;
using MarkWeb.Models;

namespace MarkWeb.Controllers
{
    public class CommandController : Controller
    {
        public ApplicationDbContext db;

        public CommandController() => db = DbSession.GetDbContext();

        // GET: Command
        public ActionResult Index()
        {
            return View(db.CommandGroup.ToList());
        }

        [HttpPost]
        public ActionResult CreateCommandGroup(string CreateDescription)
        {
            var model = new CommandGroup(CreateDescription);
            db.CommandGroup.Add(model);
            db.SaveChanges();
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditCommandGroup(string EditId, string EditDescription)
        {
            if (string.IsNullOrWhiteSpace(EditId)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = db.CommandGroup.FirstOrDefault(f => f.Id == EditId);
            if (model != null)
            {
                model.Description = EditDescription;
                db.SaveChanges();
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RemoveCommandGroup(string RemoveId)
        {
            if (string.IsNullOrWhiteSpace(RemoveId)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var model = db.CommandGroup.Include(i=>i.MarkClients).Include(i=>i.CommandContexts).FirstOrDefault(f => f.Id == RemoveId);
            if (model != null)
            {
                //if(model.CommandContexts.Any()) db.Command.RemoveRange(model.CommandContexts);
                db.Command.RemoveRange(model.CommandContexts);
                db.CommandGroup.Remove(model);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
               
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditCommand(string Id)
        {
            if (string.IsNullOrWhiteSpace(Id)) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var cg = db.CommandGroup.Include(i => i.CommandContexts).FirstOrDefault(f => f.Id == Id);
            if (cg == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            List<Command> TimingCommand = cg.CommandContexts.Where(w => w.IsRepeat == false).OrderBy(o => o.Index).ToList();
            List<Command> RepeatCommand = cg.CommandContexts.Where(w => w.IsRepeat == true).OrderBy(o => o.Index).ToList();

            string TimingText="", RepeatText="";

            foreach (var item in TimingCommand)
            {
                var cmd = item.ToString() + "\n";
                TimingText += cmd;
            }
            foreach (var item in RepeatCommand)
            {
                RepeatText += item.ToString() + "\n";
            }

            CommandViewModel model = new CommandViewModel()
            {
                CommandGroupId = Id,
                TimingText = TimingText,
                RepeatText = RepeatText
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult EditCommand(CommandViewModel model)
        {
            string[] paras = Utility.Split(model.TimingText);
            foreach (var item in paras)
            {
                Command c = Command.Parse(item);
                c.IsRepeat = false;
                c.CommandGroupId = model.CommandGroupId;
                db.Command.Add(c);
            }

            paras = Utility.Split(model.RepeatText);
            foreach (var item in paras)
            {
                Command c = Command.Parse(item);
                c.IsRepeat = true;
                c.CommandGroupId = model.CommandGroupId;
                db.Command.Add(c);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}