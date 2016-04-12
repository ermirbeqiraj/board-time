using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace MvcToDo.Models
{
    public class Initializer
    {
        /// <summary>
        /// Manage task marks, this is supposed to be called in 
        /// every application start, to create TaskMarks ( Work in progress, done, tested etc ) or TaskCategories if not pressent
        /// </summary>
        /// <returns></returns>
        public bool MngeTaskMarksAndCategories()
        {
            bool _success = false;
            try
            {
                var config = System.Configuration.ConfigurationManager.AppSettings["createDefaultTaskMarkCat"] ?? string.Empty;
                if (!string.IsNullOrEmpty(config) && config.Equals("true"))
                {
                    using (ModelContext context = new ModelContext())
                    {
                        var _taskMark = new List<TaskMark>
                    {
                        new TaskMark{ Id = 1 , Caption = "Backlog" , Active = true},
                        new TaskMark{ Id = 2 , Caption = "Ready", Active = true},
                        new TaskMark{ Id = 3 , Caption = "Work In Progress", Active = true},
                        new TaskMark{ Id = 4 , Caption = "Done", Active = true},
                        new TaskMark{ Id = 5 , Caption = "Tested", Active = true}
                    };

                        _taskMark.ForEach(t => context.TaskMark.AddOrUpdate(_taskMark.ToArray()));
                        context.SaveChanges();

                        var _taskCategories = new List<TaskCategory>
                        {
                        new TaskCategory { Id = 1 , Caption = "Feature" , Color = "label label-success" },
                        new TaskCategory { Id = 2 , Caption = "Bug" , Color = "label label-danger" }
                        };

                        _taskCategories.ForEach(t => context.TaskCategory.AddOrUpdate(_taskCategories.ToArray()));
                        context.SaveChanges();

                    }
                }
                _success = true;
            }
            catch (Exception)
            {
                /*
                 * teoricially log some errors here, may be in the future, now its weekend :)
                 */
            }
            return _success;
        }

        /// <summary>
        /// Create roles if they do not exists
        /// </summary>
        /// <returns></returns>
        public bool MngeRoles() 
        {
            bool _success = false;
            try
            {
                // Current roles suported across the application
                string[] _roles = new string[] { "admin", "projectCRUD", "taskCRUD", "taskAddFiles", "taskDeleteFiles", "taskAttachUser", "customer" };
                RoleManager<ApplicationRole> _roleManager = new RoleManager<ApplicationRole>(new RoleStore<ApplicationRole>(new ApplicationDbContext()));

                foreach (var item in _roles)
                {
                    if (!_roleManager.RoleExists(item))
                    {
                        var result = _roleManager.Create(new ApplicationRole() { Name = item });
                        if (!result.Succeeded)
                        {
                            // do some logging
                        }
                    }
                }


                var config = System.Configuration.ConfigurationManager.AppSettings["createAdminUser"] ?? string.Empty; 
                if (!string.IsNullOrEmpty(config) && !config.Equals("false"))
                {
                    // need to create the admin user, in case to log into the app
                    string email = System.Configuration.ConfigurationManager.AppSettings["defaultAdminEmail"];
                    string pass = System.Configuration.ConfigurationManager.AppSettings["defaultAdminPass"];
                    string name = System.Configuration.ConfigurationManager.AppSettings["defaultAdminName"];
                    ApplicationDbContext context = new ApplicationDbContext();
                    var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                    var userAdmin = userManager.FindByEmail(email);
                    if (userAdmin == null)
                    {
                        var user = new ApplicationUser
                        {
                            Email = email,
                            UserName = email,
                            FirstName = "Admin",
                            LastName = name
                        };
                        var result = userManager.Create(user, pass);
                        if (result.Succeeded)
                            userManager.AddToRole(user.Id, "admin");
                    }
                }
                _success = true;
            }
            catch (Exception)
            {
                // no error handling in weekend
            }
            return _success;
        }
    }
}