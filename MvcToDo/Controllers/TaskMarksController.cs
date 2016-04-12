using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MvcToDo.Models;

namespace MvcToDo.Controllers
{
    [Authorize(Roles="admin")]
    public class TaskMarksController : Controller
    {
        private ModelContext db = new ModelContext();

        // GET: TaskMarks
        public ActionResult Index()
        {
            return View(db.TaskMark.Where(x => x.Active).ToList());
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
                db.TaskMark.Add(taskMark);
                db.SaveChanges();
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
            TaskMark taskMark = db.TaskMark.Where(x => x.Active && x.Id == id).FirstOrDefault();
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
            var dbitem = db.TaskMark.Where(x => x.Active && x.Id == taskMark.Id).FirstOrDefault();
            if (dbitem == null)
                return HttpNotFound();
            
            if (ModelState.IsValid)
            {
                dbitem.Caption = taskMark.Caption;
                db.Entry(dbitem).State = EntityState.Modified;
                db.SaveChanges();
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
            TaskMark taskMark = db.TaskMark.Where(x => x.Active && x.Id == id).FirstOrDefault();
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
            TaskMark taskMark = db.TaskMark.Where(x => x.Active && x.Id == id).FirstOrDefault();
            taskMark.Active = false;
            db.Entry(taskMark).State = EntityState.Modified;
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
