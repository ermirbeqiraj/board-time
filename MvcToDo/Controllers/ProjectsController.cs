using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MvcToDo.App_Code;
using MvcToDo.Models;
using MvcToDo.ModelsView;
namespace MvcToDo.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private ModelContext db = new ModelContext();

        // GET: Projects
        public ActionResult Index()
        {
            var item = new List<SimpleList>();
            if (User.IsInRole("customer"))
            {
                // if the tables has many rows, two small selects will cost less than a 3-tables join, so
                // get the customer for this user
                string currentUserId = User.Identity.GetUserId();
                int customerId = db.CustomerUser.Where(x => x.UserId == currentUserId).Select(x => x.CustomerId).FirstOrDefault();
                // now get the projects that belong to this customer
                item = db.Project
                    .Where(x => x.CustomerId == customerId && x.Active == true)
                    .Select(x => new { Id = x.Id, Name = x.Name })
                    .AsEnumerable().Select(x => new SimpleList { Id = x.Id, Name = x.Name }).ToList();
            }
            else if (User.IsInRole("admin"))
            {
                item = (from p in db.Project
                        where  p.Active
                        select new
                        {
                            Id = p.Id,
                            Name = p.Name
                        }).Distinct()
                        .AsEnumerable()
                        .Select(x => new SimpleList { Id = x.Id, Name = x.Name }).ToList();
            }
            else
            {
                string usrId = User.Identity.GetUserId();
                item = (from p in db.Project
                        join t in db.TaskItem on p.Id equals t.ProjectId
                        join a in db.TaskAssigned on t.Id equals a.TaskId
                        where a.AssignedTo == usrId
                            && a.Active
                            && p.Active
                            && t.Active
                        select new
                        {
                            Id = p.Id,
                            Name = p.Name
                        }).Distinct()
                        .AsEnumerable()
                        .Select(x => new SimpleList { Id = x.Id, Name = x.Name }).ToList();
            }
            return View(item);
        }

        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Project project = null;
            Helpers _helper = new Helpers();
            string userId = User.Identity.GetUserId();
            if (User.IsInRole("customer"))
            {
                string currentUserId = User.Identity.GetUserId();
                int customerId = db.CustomerUser.Where(x => x.UserId == currentUserId).Select(x => x.CustomerId).FirstOrDefault();
                project = db.Project.Where(x => x.CustomerId == customerId && x.Id == id && x.Active == true).FirstOrDefault();
            }
            else if (User.IsInRole("admin") || _helper.UserAccessProject(userId, id.Value))
            {
                project = db.Project.Find(id);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            
            if (project == null)
            {
                return HttpNotFound();
            }

            project.Description = HttpUtility.HtmlDecode(project.Description);
            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "admin,projectCRUD")]
        public ActionResult Create()
        {
            ViewBag.Customers = new SelectList(db.Customer.Where(x => x.Active == true).Select(x => new { x.Id, x.Name }).ToList(), "Id", "Name", "");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,projectCRUD")]
        public ActionResult Create([Bind(Include = "Name,Description,CustomerId")] Project project)
        {
            ModelState.Clear();
            project.Active = true;
            project.Author = User.Identity.GetUserId().ToString();

            if (project.Description.Length > 3500)
            {
                ModelState.AddModelError("", "Your project description is longer than expected!");
                return View(project);
            }
            TryValidateModel(project);
            if (ModelState.IsValid)
            {
                db.Project.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("","Model state is not valid!");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "admin,projectCRUD")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Project.Where(x => x.Id == id && x.Active).FirstOrDefault();

            if (project == null)
            {
                return HttpNotFound();
            }

            project.Description = HttpUtility.HtmlDecode(project.Description);

            ViewBag.Customers = new SelectList(db.Customer.Where(x => x.Active == true).Select(x => new { x.Id, x.Name }).ToList(), "Id", "Name", project.CustomerId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,projectCRUD")]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,CustomerId")] Project project)
        {
            ModelState.Clear();
            var item = db.Project.Where(x => x.Id == project.Id && x.Active).FirstOrDefault();
            if (item != null)
            {
                item.Name = project.Name;
                item.Description = project.Description;
                item.CustomerId = project.CustomerId;
                TryValidateModel(item);
                if (ModelState.IsValid)
                {
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            else
            {
                ModelState.AddModelError("","Item with id " + project.Id + " could not be found!");
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "admin,projectCRUD")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Project.Where(x => x.Id == id && x.Active).FirstOrDefault();
            if (project == null)
            {
                return HttpNotFound();
            }

            project.Description = HttpUtility.HtmlDecode(project.Description);
            return View(project);
        }

        // POST: Projects/Delete/5
        [Authorize(Roles = "admin,projectCRUD")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Project.Find(id);
            project.Active = false;
            db.Entry(project).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
