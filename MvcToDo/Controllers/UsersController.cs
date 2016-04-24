using DbModel;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using MvcToDo.Models;
using MvcToDo.ModelsView;
using MvcToDo.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace MvcToDo.Controllers
{
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        RoleManager<ApplicationRole> _roleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(new ApplicationDbContext()));
        ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        UnitOfWork _repo;
        public UsersController()
        {
            _repo = new UnitOfWork(new ModelContext());
        }

        // GET: Users
        public ActionResult Index()
        {
            List<ApplicationUser> users = UserManager.Users.ToList();
            List<ViewUser> items = new List<ViewUser>();
            List<string> userRoles = new List<string>();
            List<string> roleNames = new List<string>();
            foreach (var item in users)
            {
                userRoles = item.Roles.Select(x => x.RoleId).ToList();
                roleNames = _roleManager.Roles.Where(x => userRoles.Contains(x.Id)).Select(x => x.Name).ToList();
                items.Add(new ViewUser { Id = item.Id, Email = item.Email, Roles = roleNames });
            }

            return View(items);
        }

        // GET: Create
        public ActionResult Create()
        {
            var items = new InsertUser();
            items.Roles = _roleManager.Roles.Select(x => new CheckBoxItem { Id = x.Id, Text = x.Name, Checked = false }).ToList();
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Email,Password,ConfirmPassword,LastName,FirstName")] InsertUser currentUser, string[] Roles)
        {
            var user = new ApplicationUser { Email = currentUser.Email, UserName = currentUser.Email, FirstName = currentUser.FirstName, LastName = currentUser.LastName };
            var result = UserManager.Create(user, currentUser.Password);
            if (result.Succeeded)
            {
                // this claim is to be displayed in the "_LoginPartial" view, as the user name instead of ugly welcome text "Hello email !"
                UserManager.AddClaim(user.Id, new Claim("MvcToDo:UserFullName", currentUser.FirstName + " " + currentUser.LastName));
                var currentRole = new ApplicationRole();
                if (Roles != null)
                {
                    string[] roleNames = _roleManager.Roles.Where(x => Roles.Contains(x.Id)).Select(x => x.Name).ToArray();
                    UserManager.AddToRoles(user.Id, roleNames);
                }
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", result.Errors.FirstOrDefault());
            currentUser.Roles = _roleManager.Roles.Select(x => new CheckBoxItem { Id = x.Id, Text = x.Name, Checked = false }).ToList();
            return View(currentUser);

        }

        public ActionResult Edit(string Id)
        {
            var item = GetEditUser(Id);
            var customers = _repo.Customer.Find(x => x.Active == true).Select(x => new { x.Id, x.Name }).ToList();
            int? companyId = _repo.CustomerUser.Find(x => x.UserId == Id).Select(x => x.CustomerId).FirstOrDefault();
            ViewBag.Customer = new SelectList(customers, "Id", "Name", companyId.HasValue ? companyId.Value : -1);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Email,FirstName,LastName")] EditUser modelUser, string[] Roles, bool chkIsCustomer, int? CustomerId)
        {
            if (ModelState.IsValid)
            {
                var dbUser = UserManager.Users.Where(x => x.Id == modelUser.Id).FirstOrDefault();
                if (dbUser != null)
                {
                    // delete roles that involve this user
                    string[] currentRoles = dbUser.Roles.Select(r => r.RoleId).ToArray();
                    string[] dbRoles = _roleManager.Roles.Where(x => currentRoles.Contains(x.Id)).Select(x => x.Name).ToArray();
                    UserManager.RemoveFromRoles(dbUser.Id, dbRoles);
                    // update name & email
                    dbUser.FirstName = modelUser.FirstName;
                    dbUser.LastName = modelUser.LastName;
                    dbUser.Email = modelUser.Email;
                    var result = UserManager.Update(dbUser);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", "Update Failed!");
                        return View(modelUser);
                    }

                    var claims = UserManager.GetClaims(dbUser.Id).Where(c => c.Type == "MvcToDo:UserFullName").ToList();
                    foreach (var item in claims)
                    {
                        UserManager.RemoveClaim(dbUser.Id, item);
                    }

                    UserManager.AddClaim(dbUser.Id, new Claim("MvcToDo:UserFullName", dbUser.FirstName + " " + dbUser.LastName));


                    // get our roles
                    if (Roles != null)
                    {
                        var requiredRoles = _roleManager.Roles.Where(x => Roles.Contains(x.Id)).Select(x => x.Name).ToArray();
                        UserManager.AddToRoles(dbUser.Id, requiredRoles);
                    }

                    // get all rows where this user is customer

                    var customerUser = _repo.CustomerUser.Find(x => x.UserId == modelUser.Id).ToList();
                    _repo.CustomerUser.RemoveRange(customerUser);

                    if (chkIsCustomer && CustomerId.HasValue)
                    {
                        CustomerUser cu = new CustomerUser { UserId = modelUser.Id, CustomerId = CustomerId.Value, Active = true };
                        _repo.CustomerUser.Add(cu);
                        _repo.Persist();
                        UserManager.AddToRole(dbUser.Id, "customer");
                    }


                    return RedirectToAction("Index");
                }
                else
                {
                    return HttpNotFound();
                }
            }
            else
            {
                ModelState.AddModelError("", "Model state is not valid!");
                var item = GetEditUser(modelUser.Id);
                modelUser.Roles = item.Roles;
                return View(modelUser);
            }
        }

        public ActionResult ChangePassword(string Id)
        {
            var currentUser = UserManager.Users.Where(x => x.Id == Id).FirstOrDefault();
            if (currentUser != null)
            {
                ChangePassword modelUser = new ChangePassword();
                modelUser.Id = Id;
                return View(modelUser);
            }
            else
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword([Bind(Include = "NewPassword,Id,ConfirmPassword")] ChangePassword model)
        {
            var currentUser = UserManager.Users.Where(x => x.Id == model.Id).FirstOrDefault();
            if (currentUser != null)
            {
                var resultRemove = UserManager.RemovePassword(currentUser.Id);
                var resultAdd = UserManager.AddPassword(currentUser.Id, model.NewPassword);
                if (resultAdd.Succeeded && resultRemove.Succeeded)
                    return RedirectToAction("Index");
                else
                {
                    ModelState.AddModelError("", resultAdd.Errors.FirstOrDefault());
                    ModelState.AddModelError("", resultRemove.Errors.FirstOrDefault());
                }
            }
            else
            {
                ModelState.AddModelError("", "Cant find user!");
            }
            return View(model);
        }

        public ActionResult Delete(string Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var currentUser = UserManager.Users.Where(x => x.Id == Id).Select(x => new ViewUser { Id = x.Id, Email = x.Email }).FirstOrDefault();
            if (currentUser != null)
            {
                return View(currentUser);
            }
            else
            {
                return HttpNotFound();
            }
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string Id)
        {
            var currentUser = UserManager.Users.Where(x => x.Id == Id).FirstOrDefault();
            if (currentUser != null)
            {
                var result = UserManager.Delete(currentUser);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", result.Errors.FirstOrDefault());
                    return View(currentUser);
                }
            }
            else
                return HttpNotFound();
        }


        #region Helpers
        private EditUser GetEditUser(string Id)
        {
            var dbUser = UserManager.Users.Where(x => x.Id == Id).FirstOrDefault();
            var currentRoles = dbUser.Roles.Select(x => x.RoleId).ToList();
            var allRoles = _roleManager.Roles.ToList();

            EditUser editUser = new EditUser();
            foreach (var x in allRoles)
            {
                if (currentRoles.Contains(x.Id))
                    editUser.Roles.Add(new CheckBoxItem { Id = x.Id, Text = x.Name, Checked = true });
                else
                    editUser.Roles.Add(new CheckBoxItem { Id = x.Id, Text = x.Name, Checked = false });
            }

            editUser.FirstName = dbUser.FirstName;
            editUser.LastName = dbUser.LastName;
            editUser.Email = dbUser.Email;
            editUser.Id = dbUser.Id;
            return editUser;
        }
        #endregion

    }
}