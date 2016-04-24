using DbModel;
using MvcToDo.App_Code;
using MvcToDo.Persistence;
using System.Net;
using System.Web.Mvc;

namespace MvcToDo.Controllers
{
    [Authorize(Roles = "admin")]
    public class TaskCategoriesController : Controller
    {
        UnitOfWork _repo;
        Helpers _helper;
        public TaskCategoriesController()
        {
            _repo = new UnitOfWork(new ModelContext());
            _helper = new Helpers();
        }

        // GET: TaskCategories
        public ActionResult Index()
        {
            var items = _repo.TaskCategory.GetAll();
            return View(items);
        }

        // GET: TaskCategories/Create
        public ActionResult Create()
        {
            ViewBag.ColorsCat = new SelectList(_helper.GetCategoryColor(), "Name", "Name");
            return View();
        }

        // POST: TaskCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Caption,Color")] TaskCategory taskCategory)
        {
            if (ModelState.IsValid)
            {
                _repo.TaskCategory.Add(taskCategory);
                _repo.Persist();
                return RedirectToAction("Index");
            }
            return View(taskCategory);
        }

        // GET: TaskCategories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskCategory taskCategory = _repo.TaskCategory.Get(id.Value);
            if (taskCategory == null)
            {
                return HttpNotFound();
            }
            ViewBag.ColorsCat = new SelectList(_helper.GetCategoryColor(), "Name", "Name");
            return View(taskCategory);
        }

        // POST: TaskCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Caption,Color")] TaskCategory taskCategory)
        {
            if (ModelState.IsValid)
            {
                _repo.TaskCategory.Update(taskCategory);
                _repo.Persist();
                return RedirectToAction("Index");
            }
            return View(taskCategory);
        }

        // GET: TaskCategories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskCategory taskCategory = _repo.TaskCategory.Get(id.Value);
            if (taskCategory == null)
            {
                return HttpNotFound();
            }
            return View(taskCategory);
        }

        // POST: TaskCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TaskCategory taskCategory = _repo.TaskCategory.Get(id);
            _repo.TaskCategory.Remove(taskCategory);
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
