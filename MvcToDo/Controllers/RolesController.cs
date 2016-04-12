using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using MvcToDo.Models;
using MvcToDo.ModelsView;

namespace MvcToDo.Controllers
{
    [Authorize(Roles = "admin")]
    public class RolesController : Controller
    {
        RoleManager<ApplicationRole> _roleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(new ApplicationDbContext()));
        // GET: Roles
        public ActionResult Index()
        {
            var items = _roleManager.Roles.Select(x => new ViewRole { Id = x.Id, Name = x.Name }).ToList();
            return View(items);
        }

        // GET: Roles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name")] ViewRole role)
        {
            if (ModelState.IsValid)
            {
                var x = _roleManager.Create(new ApplicationRole { Name = role.Name  });
                if (x.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Could not create role! \n"
                        + x.Errors.FirstOrDefault());
                }
            }
            return View();
        }

        // GET: Roles/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var item = _roleManager.Roles.Where(x => x.Id == id).Select(x => new ViewRole { Id = x.Id, Name = x.Name }).FirstOrDefault();
            if (item == null)
                return HttpNotFound();

            return View(item);
        }

        // POST: Roles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name")] ViewRole role)
        {
            if (ModelState.IsValid)
            {
                var currentRole = _roleManager.Roles.FirstOrDefault(x => x.Id == role.Id);
                if (currentRole != null)
                {
                    currentRole.Name = role.Name;
                    var result = _roleManager.Update(currentRole);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Could not update the role!" + result.Errors.FirstOrDefault());
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Role does not exist!");
                }
            }
            return View();
        }

        // GET: Roles/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var currentRole = _roleManager.Roles.Select(x => new ViewRole { Id = x.Id, Name = x.Name })
                .FirstOrDefault(x => x.Id == id);
            if (currentRole == null)
            {
                return HttpNotFound();
            }
            return View(currentRole);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var currentRole = _roleManager.Roles.FirstOrDefault(x => x.Id == id);
            if (currentRole != null)
            {
                var result = _roleManager.Delete(currentRole);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Could not delete the role!" + result.Errors.FirstOrDefault());
                }
            }
            else
            {
                ModelState.AddModelError("", "Role does not exist!");
            }

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _roleManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}