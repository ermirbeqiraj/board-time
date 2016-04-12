using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MvcToDo.App_Code;
using MvcToDo.Models;

namespace MvcToDo.Controllers
{
    [Authorize(Roles = "admin")]
    public class TaskCategoriesController : Controller
    {
        private ModelContext db = new ModelContext();
        Helpers _helper = new Helpers();

        // GET: TaskCategories
        public ActionResult Index()
        {
            var items = (from x in db.TaskCategory
                         select x).ToList();
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
                db.TaskCategory.Add(taskCategory);
                db.SaveChanges();
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
            TaskCategory taskCategory = db.TaskCategory.Find(id);
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
                db.Entry(taskCategory).State = EntityState.Modified;
                db.SaveChanges();
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
            TaskCategory taskCategory = db.TaskCategory.Find(id);
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
            TaskCategory taskCategory = db.TaskCategory.Find(id);
            db.TaskCategory.Remove(taskCategory);
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
