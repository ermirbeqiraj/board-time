using System.Linq;
using System.Net;
using System.Web.Mvc;
using MvcToDo.Persistence;
using DbModel;

namespace MvcToDo.Controllers
{
    [Authorize(Roles="admin")]
    public class TaskMarksController : Controller
    {
        UnitOfWork _repo;
        public TaskMarksController()
        {
            _repo = new UnitOfWork(new ModelContext());
        }
        
        // GET: TaskMarks
        public ActionResult Index()
        {
            return View(_repo.TaskMark.Find(x => x.Active).ToList());
        }

        // GET: TaskMarks/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TaskMarks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Caption")] TaskMark taskMark)
        {
            taskMark.Active = true;
            if (ModelState.IsValid)
            {
                _repo.TaskMark.Add(taskMark);
                _repo.Persist();
                return RedirectToAction("Index");
            }

            return View(taskMark);
        }

        // GET: TaskMarks/Edit/5
        public ActionResult Edit(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskMark taskMark = _repo.TaskMark.GetSingle(x => x.Active && x.Id == id.Value);
            if (taskMark == null)
            {
                return HttpNotFound();
            }
            return View(taskMark);
        }

        // POST: TaskMarks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Caption")] TaskMark taskMark)
        {
            var dbitem = _repo.TaskMark.GetSingle(x => x.Active && x.Id == taskMark.Id);
            if (dbitem == null)
                return HttpNotFound();
            
            if (ModelState.IsValid)
            {
                dbitem.Caption = taskMark.Caption;
                _repo.TaskMark.Update(dbitem);
                _repo.Persist();
                return RedirectToAction("Index");
            }
            return View(taskMark);
        }

        // GET: TaskMarks/Delete/5
        public ActionResult Delete(byte? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskMark taskMark = _repo.TaskMark.GetSingle(x => x.Active && x.Id == id);
            if (taskMark == null)
            {
                return HttpNotFound();
            }
            return View(taskMark);
        }

        // POST: TaskMarks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(byte id)
        {
            TaskMark taskMark = _repo.TaskMark.GetSingle(x => x.Active && x.Id == id);
            taskMark.Active = false;
            _repo.TaskMark.Update(taskMark);
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
