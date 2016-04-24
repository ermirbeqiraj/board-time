using DbModel;
using Microsoft.AspNet.Identity;
using MvcToDo.ModelsView;
using MvcToDo.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MvcToDo.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        UnitOfWork _repo;
        public ProjectsController()
        {
            _repo = new UnitOfWork(new ModelContext());
        }
        // GET: Projects
        public ActionResult Index()
        {
            var item = new List<SimpleList>();
            if (User.IsInRole("customer"))
            {
                // if the tables has many rows, two small selects will cost less than a 3-tables join, so
                // get the customer for this user
                string currentUserId = User.Identity.GetUserId();
                int customerId = _repo.CustomerUser.GetCustomerIdByUserId(currentUserId);
                // now get the projects that belong to this customer
                item = _repo.Project.GetProjectsByCustomerId(customerId).ToList();
            }
            else if (User.IsInRole("admin"))
            {
                item = _repo.Project.GetAllProjectSimpleList(true).ToList();
            }
            else
            {
                item = _repo.Project.GetUserProjects(User.Identity.GetUserId()).ToList();
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
            string userId = User.Identity.GetUserId();
            if (User.IsInRole("customer"))
            {
                string currentUserId = User.Identity.GetUserId();
                int customerId = _repo.CustomerUser.GetCustomerIdByUserId(currentUserId);
                project = _repo.Project.GetSingle(x => x.CustomerId == customerId && x.Id == id.Value && x.Active);
            }
            else if (User.IsInRole("admin") || _repo.TaskAssigned.UserAccessProject(userId, id.Value))
            {
                project = _repo.Project.Get(id.Value);
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
            var customers = _repo.Customer.Find(x => x.Active).AsEnumerable().Select(x => new { x.Id, x.Name }).ToList();
            ViewBag.Customers = new SelectList(customers, "Id", "Name", "");
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
                _repo.Project.Add(project);
                _repo.Persist();
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
            Project project = _repo.Project.GetSingle(x => x.Active && x.Id == id.Value);

            if (project == null)
            {
                return HttpNotFound();
            }

            project.Description = HttpUtility.HtmlDecode(project.Description);
            var customers = _repo.Customer.Find(x => x.Active).AsEnumerable().Select(x => new { x.Id, x.Name }).ToList();
            ViewBag.Customers = new SelectList(customers, "Id", "Name", project.CustomerId);
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
            var item = _repo.Project.GetSingle(x => x.Active && x.Id == project.Id);
            if (item != null)
            {
                item.Name = project.Name;
                item.Description = project.Description;
                item.CustomerId = project.CustomerId;
                TryValidateModel(item);
                if (ModelState.IsValid)
                {
                    _repo.Project.Update(item);
                    _repo.Persist();
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
            Project project = _repo.Project.GetSingle(x => x.Active && x.Id == id.Value);
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
            Project project = _repo.Project.GetSingle(x => x.Active && x.Id == id);
            project.Active = false;
            _repo.Project.Update(project);
            _repo.Persist();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
