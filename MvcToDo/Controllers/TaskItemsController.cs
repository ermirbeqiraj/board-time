using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MvcToDo.App_Code;
using MvcToDo.Models;
using MvcToDo.ModelsView;

namespace MvcToDo.Controllers
{
    [Authorize]
    public class TaskItemsController : Controller
    {
        private ModelContext db = new ModelContext();
        Helpers _helper = new Helpers();
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

        // GET: TaskItems
        public ActionResult Index(int? CategoryId, DateTime? DueDate, string UserId = "")
        {
            List<TaskList> taskItem = null;
            if (RouteData.Values["projectId"] != null)
            {
                int projectId = int.Parse(RouteData.Values["projectId"].ToString());
                if (User.IsInRole("admin") || _helper.UserAccessProject(User.Identity.GetUserId(), projectId))
                {
                    taskItem = _helper.GetTasksForList(projectId, CategoryId, DueDate, UserId);
                }
                else
                {
                    return HttpNotFound();
                }
            }
            else
            {
                return HttpNotFound();
            }

            //Get TaskCategories and Users for filtering
            var Categories = db.TaskCategory.Select(x => new { Id = x.Id, Name = x.Caption }).AsEnumerable().Select(x => new SimpleList { Id = x.Id, Name = x.Name }).ToList();
            var UsersDrop = UserManager.Users.Select(x => new StringList { Id = x.Id, Name = (x.FirstName + " " + x.LastName) }).ToList();
            ViewBag.TCategories = new SelectList(Categories, "Id", "Name",CategoryId);
            ViewBag.TUsers = new SelectList(UsersDrop, "Id", "Name",  UserId);

            return View(taskItem);
        }

        // GET: TaskItems/Board
        public ActionResult Board(int? CategoryId, DateTime? DueDate, string UserId = "")
        {
            var route = RouteData.Values["projectId"];
            if (route == null)
                return HttpNotFound();

            BoardView viewItems = new BoardView();
            int projectId;
            if (int.TryParse(route.ToString(), out projectId) && (User.IsInRole("admin") || _helper.UserAccessProject(User.Identity.GetUserId(), projectId)))
            {
                viewItems.BoardColumns = db.TaskMark.Where(x => x.Active).ToList();
                viewItems.Board = _helper.GetTaskByProjectId(projectId, CategoryId, DueDate, UserId);
                var Categories = db.TaskCategory.Select(x => new { Id = x.Id, Name = x.Caption }).AsEnumerable().Select(x => new SimpleList { Id = x.Id, Name = x.Name }).ToList();
                var UsersDrop = UserManager.Users.Select(x => new StringList { Id = x.Id, Name = (x.FirstName + " " + x.LastName) }).ToList();
                viewItems.TCategories = new SelectList(Categories, "Id", "Name", CategoryId);
                viewItems.TUsers = new SelectList(UsersDrop, "Id", "Name", UserId);
                return View(viewItems);
            }
            else if(int.TryParse(route.ToString(), out projectId) && (User.IsInRole("customer")))
            {
                string currentUserId = User.Identity.GetUserId();
                int customerId = db.CustomerUser.Where(x => x.UserId == currentUserId).Select(x => x.CustomerId).FirstOrDefault();
                var projects = db.Project.Where(x => x.CustomerId == customerId && x.Active == true).Select(x => x.Id).ToList();
                if (projects.Contains(projectId))
                {
                    viewItems.BoardColumns = db.TaskMark.Where(x => x.Active).ToList();
                    viewItems.Board = _helper.GetTaskByProjectId(projectId, CategoryId, DueDate, UserId);
                    var Categories = db.TaskCategory.Select(x => new { Id = x.Id, Name = x.Caption }).AsEnumerable().Select(x => new SimpleList { Id = x.Id, Name = x.Name }).ToList();
                    var UsersDrop = UserManager.Users.Select(x => new StringList { Id = x.Id, Name = (x.FirstName + " " + x.LastName) }).ToList();
                    viewItems.TCategories = new SelectList(Categories, "Id", "Name", CategoryId);
                    viewItems.TUsers = new SelectList(UsersDrop, "Id", "Name", UserId);
                    return View(viewItems);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
            }
            else
            {
                return HttpNotFound();
            }
        }

        /// <summary>
        /// Remember the column-on-board for the task
        /// </summary>
        /// <param name="id">Task Id</param>
        /// <param name="mark">Task Mark</param>
        /// <returns></returns>        
        [HttpPost]
        public JsonResult BoardData(string id, string mark, string from)
        {
            string result = "";
            try
            {
                id = id.Replace("item_", "").Trim();
                mark = mark.Replace("col_", "").Trim();
                from = from.Replace("col_", "").Trim();

                int _taskId;
                byte _markId , _from;
                if (int.TryParse(id, out _taskId) && byte.TryParse(mark, out _markId) && byte.TryParse(from,out _from) && (mark != from))
                {
                    /*
                     * a task can be moved between columns just by the admin or by a user that is assigned to that task
                     */
                    string currentUser = User.Identity.GetUserId();
                    var res = db.TaskAssigned.Any(x => x.TaskId == _taskId && x.AssignedTo == currentUser);
                    if (res || User.IsInRole("admin"))
                    {
                        using (var _db = new ModelContext())
                        {
                            var item = _db.TaskItem.Where(x => x.Id == _taskId).FirstOrDefault();
                            if (item != null)
                            {
                                item.Mark = _markId;
                                _db.Entry(item).State = EntityState.Modified;
                                _db.SaveChanges();
                                result = "1";

                                TaskLifecycle taskLifecycle = new TaskLifecycle { Actor = currentUser , TaskId = _taskId , Created = DateTime.Now , MarkFromId = _from , MarkToId = _markId };
                                _db.TaskLifecycle.Add(taskLifecycle);
                                _db.SaveChanges();
                            }
                            else
                            {
                                result = "Item could not be found!";
                            }
                        }
                    }
                    else
                    {
                        result = "Failed : You are not authorized to do this action ";
                    }
                }
            }
            catch (Exception ex)
            {
                result = "Failed : " + ex.Message;
            }
            return Json(result, JsonRequestBehavior.DenyGet);
        }

        // GET: TaskItems/Details/5
        public ActionResult Details(int? id)
        {
            // get task
            TaskItem taskItem = db.TaskItem.Find(id);
            if (taskItem == null)
            {
                return HttpNotFound();
            }

            // ensure user has the rights to access this task
            #region access
            int projectId = taskItem.ProjectId;
            string userId = User.Identity.GetUserId();
            if (!User.IsInRole("admin") && !_helper.UserAccessProject(userId, projectId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            #endregion
            // get comments
            var comments = taskItem.Comments.ToList();
            ApplicationDbContext _db = new ApplicationDbContext();
            foreach (var x in comments)
            {
                string currId = x.Author;
                var userCom = _db.Users.Where(u => u.Id == currId).FirstOrDefault();
                x.Author = userCom != null ? (userCom.FirstName + " " + userCom.LastName) : "Not Found!";
                x.Comment = HttpUtility.HtmlDecode(x.Comment);
            }

            //get files
            var atFiles = (from f in db.Files
                           join tf in db.TaskFiles on f.Id equals tf.FileId
                           join t in db.TaskItem on tf.TaskId equals t.Id
                           where t.Id == taskItem.Id
                           select f).ToList();
            // get assignedTo
            string[] users = db.TaskAssigned.Where(x => x.TaskId == id).Select(x => x.AssignedTo).ToArray();
            var atUsers = _db.Users.Where(x => users.Contains(x.Id)).ToList();
            string allUsers = "";
            foreach (var x in atUsers)
            {
                allUsers += x.FirstName + " " + x.LastName + " ,";
            }

            TaskItemView currTask = new TaskItemView(comments, allUsers, atFiles, taskItem);
            currTask.TaskMark = taskItem.TaskMark.Caption;
            currTask.ProjectName = taskItem.Project.Name;
            string curraId = taskItem.Author;
            var currUser = _db.Users.Where(u => u.Id == curraId).FirstOrDefault();
            currTask.Author = currUser != null ? (currUser.FirstName + " " + currUser.LastName) : "Not Found!";

            // parent task:
            ViewBag.ParentTask = taskItem.ParentId == null ? "-" : db.TaskItem.Find(taskItem.ParentId).Name;
            currTask.ParentId = taskItem.ParentId;

            // visibility of sections
            ViewBag.totalComments = currTask.Comments.Count().ToString();
            ViewBag.totalFiles = currTask.UploadedFiles.Count().ToString();

            return View(currTask);
        }

        // GET: TaskItems/DownloadFile/5?FileId=3
        public FileResult DownloadFile(int FileId, int Id)
        {
            #region access
            TaskItem taskItem = db.TaskItem.Find(Id);
            if (taskItem == null)
                throw new HttpException(404, "File not found");

            // ensure user has the rights to access this task
            int projectId = taskItem.ProjectId;
            string userId = User.Identity.GetUserId();
            if (!User.IsInRole("admin") && !_helper.UserAccessProject(userId, projectId))
            {
                throw new HttpException(403, "not authorized results!");
            }
            #endregion

            var item = db.Files.Where(x => x.Id == FileId).FirstOrDefault();
            if (item != null)
            {
                return File(item.FileContent, item.ContentType, item.FileName);
            }
            else
            {
                throw new HttpException(404, "File not found");
            }
        }

        // POST verb to add a new file for a specificc task
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment([Bind(Include = "Id,Comment")] int Id, string Comment)
        {
            TaskItem taskItem = db.TaskItem.Find(Id);
            if (taskItem == null)
                return HttpNotFound();

            // ensure user has the rights to access this task
            int projectId = taskItem.ProjectId;
            string userId = User.Identity.GetUserId();
            if (!User.IsInRole("admin") && !_helper.UserAccessProject(userId, projectId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var item = new Comments()
            {
                Active = true,
                Author = User.Identity.GetUserId(),
                Comment = Comment,
                Created = DateTime.Now,
                TaskId = Id
            };
            ModelState.Clear();
            TryValidateModel(item);
            if (ModelState.IsValid)
            {
                db.Comments.Add(item);
                db.SaveChanges();
            }
            else
            {
                ModelState.AddModelError("", "Model is not valid!");
            }

            return RedirectToAction("Details", new { id = Id });
        }

        #region Files
        //GET: /TaskItems/Files/1
        public ActionResult Files(int Id)
        {
            #region access
            TaskItem taskItem = db.TaskItem.Find(Id);
            if (taskItem == null)
                return HttpNotFound();

            // ensure user has the rights to access this task
            int projectId = taskItem.ProjectId;
            string userId = User.Identity.GetUserId();
            if (!User.IsInRole("admin") && !_helper.UserAccessProject(userId, projectId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            #endregion

            // task id is nessesary for getting files for the right task
            var items = (from f in db.Files
                         join tf in db.TaskFiles on f.Id equals tf.FileId
                         join t in db.TaskItem on tf.TaskId equals t.Id
                         where t.Id == Id
                         select f).ToList();
            ViewBag.TaskId = Id;
            return View(items);
        }

        // get TaskItem/AddFile/5
        [Authorize(Roles = "admin,taskAddFiles")]
        public ActionResult AddFile(int id)
        {
            //GET verb to the view for adding a new file to the task
            var task = db.TaskItem.Where(x => x.Id == id).FirstOrDefault();
            if (task != null)
            {
                ViewBag.TaskId = task.Id;
                ViewBag.TaskName = task.Name;
            }
            return View();
        }

        [Authorize(Roles = "admin,taskAddFiles")]
        public ActionResult CopyFile(int id) 
        {
            ViewBag.TaskId = id;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "admin,taskCRUD")]
        public JsonResult GetFiles(string taskUrl)
        {
            //taskUrl : http://localhost:49845/Project-2/TaskItems/CopyFile/9
            var result = new List<SimpleList>();

            if (taskUrl == null)
            {
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            string[] items = taskUrl.Split(new string[] { "/" } ,  StringSplitOptions.RemoveEmptyEntries);

            if (items.Length > 0)
            {
                // get last item
                int taskId = int.Parse(items[items.Length - 1]);
                result = (from files in db.Files
                                 join tfiles in db.TaskFiles on files.Id equals tfiles.FileId
                                 where tfiles.Active == true
                                 select new
                                 {
                                     Id = files.Id,
                                     Name = files.FileName
                                 }
                             ).AsEnumerable().Select(x => new SimpleList { Id = x.Id, Name = x.Name }).ToList();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,taskAddFiles")]
        public ActionResult CopyFile([Bind(Include = "fileId,toTaskId")] int fileId, int toTaskId)
        {
            var file = db.Files.Where(x => x.Id == fileId).FirstOrDefault();
            if (file == null)
            {
                return HttpNotFound();
            }

            TaskFiles tf = new TaskFiles { FileId = file.Id, TaskId = toTaskId, Active = true };
            db.TaskFiles.Add(tf);
            db.SaveChanges();

            return RedirectToAction("Details", new { id = toTaskId });
        }

        // POST verb to add a new file for a specificc task
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,taskAddFiles")]
        public ActionResult AddFile([Bind(Include = "Id")] int Id, HttpPostedFileBase upload)
        {
            try
            {
                if (upload != null && upload.ContentLength > 0)
                {
                    // get main properties
                    var item = new Files
                    {
                        FileName = System.IO.Path.GetFileName(upload.FileName),
                        ContentType = upload.ContentType,
                        Author = User.Identity.GetUserId(),
                        Active = true,
                        Created = DateTime.Now
                    };
                    // set file content
                    using (var reader = new System.IO.BinaryReader(upload.InputStream))
                    {
                        item.FileContent = reader.ReadBytes(upload.ContentLength);
                    }

                    ModelState.Clear();
                    TryValidateModel(item);
                    if (ModelState.IsValid)
                    {
                        // Add Files
                        db.Files.Add(item);
                        db.SaveChanges();
                        var tfiles = new TaskFiles { TaskId = Id, FileId = item.Id, Active = true };
                        //GetId
                        //Add TaskFiles
                        db.TaskFiles.Add(tfiles);
                        db.SaveChanges();
                        return RedirectToAction("Details", new { id = Id });
                    }
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(Id);
        }


        //GET: /TaskItems/DeleteFile/3
        [Authorize(Roles = "admin,taskDeleteFiles")]
        public ActionResult DeleteFile(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Please send an accepted request and stop playing around!");
            }
            var item = db.Files.Where(x => x.Id == id).FirstOrDefault();
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // POST: TaskItems/DeleteFile/3
        [HttpPost, ActionName("DeleteFile")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,taskDeleteFiles")]
        public ActionResult DeleteFiles([Bind(Include = "Id")] int id)
        {
            using (var _db = new ModelContext())
            {
                var item = _db.Files.Where(x => x.Id == id).FirstOrDefault();
                if (item != null)
                {
                    var taskFiles = _db.TaskFiles.Where(x => x.FileId == item.Id).ToList();
                    foreach (var i in taskFiles)
                    {
                        _db.Entry(i).State = EntityState.Deleted;
                    }
                    _db.SaveChanges();
                    _db.Entry(item).State = EntityState.Deleted;
                    _db.SaveChanges();
                    return RedirectToAction("Files", new { id = id });
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }

        #endregion

        #region Users
        // Get : TaskItems/Users/1
        [Authorize(Roles = "admin,taskAttachUser")]
        public ActionResult Users(int Id)
        {
            var _db = new ApplicationDbContext();
            // get assignedTo
            string[] assignedUsers = db.TaskAssigned.Where(x => x.TaskId == Id).Select(x => x.AssignedTo).ToArray();
            var atUsers = _db.Users.Where(x => assignedUsers.Contains(x.Id))
                .Select(x => new UserChecks() { Id = x.Id, Name = x.UserName, Checked = true }).ToList();

            var notAssigned = _db.Users.Where(x => !assignedUsers.Contains(x.Id))
                .Select(x => new UserChecks() { Id = x.Id, Name = x.UserName, Checked = false });
            atUsers.AddRange(notAssigned);
            var arg = new AtachUserToTask() { Id = Id, UsersToAdd = atUsers };
            // attach users in this task
            return View(arg);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,taskAttachUser")]
        public ActionResult Users([Bind(Include = "Id,UsersToAdd")] int Id, string[] UsersToAdd)
        {
            try
            {
                TaskAssigned taskAssigned = null;
                string currentAuthor = User.Identity.GetUserId();
                var itemstodel = db.TaskAssigned.Where( x => x.TaskId == Id).ToList();
                db.TaskAssigned.RemoveRange(itemstodel);
                db.SaveChanges();
                foreach (var item in UsersToAdd)
                {
                    taskAssigned = new TaskAssigned
                    {
                        Active = true,
                        AssignedTo = item,
                        Author = currentAuthor,
                        TaskId = Id
                    };
                    db.TaskAssigned.Add(taskAssigned);
                    db.SaveChanges();
                }
                return RedirectToAction("Details", new { id = Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Exception", ex.Message);
            }

            ModelState.AddModelError("", "Insertion Failed!");

            // failure
            var _db = new ApplicationDbContext();
            var users = _db.Users.Select(x => new UserChecks() { Id = x.Id, Name = x.UserName }).ToList();
            var arg = new AtachUserToTask() { Id = Id, UsersToAdd = users };
            return View(arg);
        }
        #endregion

        #region Create/Dublicate Task
        // GET: TaskItems/Create
        [Authorize(Roles = "admin,taskCRUD")]
        public ActionResult Create(int? Parent)
        {
            var projectId = "-1";
            if (RouteData.Values["projectId"] != null)
            {
                projectId = RouteData.Values["projectId"].ToString();
            }

            var parents = db.TaskItem.Where(x => x.Active == true);
            ViewBag.ProjectCollection = new SelectList(db.Project, "Id", "Name", projectId);
            ViewBag.Mark = new SelectList(db.TaskMark, "Id", "Caption");
            ViewBag.ParentCollection = new SelectList(parents, "Id", "Name", (Parent.HasValue ? Parent.Value : -1));
            ViewBag.PriorityBag = new SelectList(_helper.GetTaskPriority(), "Id", "Name","3");
            ViewBag.Taskcategory = new SelectList(db.TaskCategory.ToList(), "Id", "Caption");
            return View();
        }

        // POST: TaskItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,taskCRUD")]
        public ActionResult Create([Bind(Include = "Name,Description,ParentId,Category,ProjectId,DueDate,Priority,StartDate,TimeEstimated,Mark")] TaskItem taskItem)
        {
            int projectId;
            // if project is missing or somehow we cant get the project id, dont go further
            if ((RouteData.Values["projectId"] == null) || (!int.TryParse(RouteData.Values["projectId"].ToString(),out projectId)))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            taskItem.ProjectId = projectId;
            taskItem.Status = true; // active / innactive  in board
            taskItem.Created = DateTime.Now;
            taskItem.LastModified = taskItem.Created;
            taskItem.Author = User.Identity.GetUserId();
            taskItem.Active = true;
            ModelState.Clear();
            TryValidateModel(taskItem);

            if (ModelState.IsValid)
            {
                db.TaskItem.Add(taskItem);
                db.SaveChanges();
                return RedirectToAction("Details", new { id = taskItem.Id});
            }

            ViewBag.ProjectId = new SelectList(db.Project, "Id", "Name", taskItem.ProjectId);
            ViewBag.Mark = new SelectList(db.TaskMark, "Id", "Caption", taskItem.Mark);
            ViewBag.ParentId = new SelectList(db.TaskItem.Where(x => x.Active == true), "Id", "Name", taskItem.ParentId);
            ViewBag.Priority = new SelectList(_helper.GetTaskPriority(), "Id", "Name", taskItem.Priority);

            return View(taskItem);
        }

        // GET: TaskItems/Dublicate/5
        [Authorize(Roles = "admin,taskCRUD")]
        public ActionResult Dublicate(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            DublicateTask modelItem = new DublicateTask();
            modelItem.TaskId = id.Value;
            string taskName = db.TaskItem.Where(x => x.Id == id.Value).Select(x => x.Name).FirstOrDefault();
            if (!string.IsNullOrEmpty(taskName))
                modelItem.TaskName = taskName + " (Copy)";
            else
                return HttpNotFound();

            return View(modelItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,taskCRUD")]
        public ActionResult Dublicate([Bind(Include = "TaskId,TaskName,IncludeFiles,IncludeComments,IncludeUsers")] DublicateTask model)
        {
            if (ModelState.IsValid)
            {
                var newTask = _helper.GetNewTaskForDublicate(model, User.Identity.GetUserId());
                if (newTask != null)
                {
                    db.TaskItem.Add(newTask);
                    db.SaveChanges();
                }
                else
                {
                    return HttpNotFound();
                }
                return RedirectToAction("Details", new { id = newTask.Id });
            }
            else
            {
                ModelState.AddModelError("", "Model is not valid!");
                return View(model);
            }
        }

        #endregion

        #region Edit Task
        // GET: TaskItems/Edit/5
        [Authorize(Roles = "admin,taskCRUD")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskItem taskItem = db.TaskItem.Find(id);
            if (taskItem == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProjectId = new SelectList(db.Project, "Id", "Name", taskItem.ProjectId);
            ViewBag.Mark = new SelectList(db.TaskMark, "Id", "Caption", taskItem.Mark);
            ViewBag.ParentId = new SelectList(db.TaskItem.Where(x => x.Active == true), "Id", "Name", taskItem.ParentId ?? 0);
            ViewBag.PriorityBag = new SelectList(_helper.GetTaskPriority(), "Id", "Name", taskItem.Priority);
            ViewBag.DropCat = new SelectList(db.TaskCategory.ToList(), "Id", "Caption", taskItem.Category);

            return View(taskItem);
        }


        // POST: TaskItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,taskCRUD")]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,ParentId,Category,ProjectId,DueDate,Priority,StartDate,TimeEstimated,Mark,Completed")] TaskItem taskItem)
        {
            var dbitem = db.TaskItem.Where(x => x.Id == taskItem.Id).FirstOrDefault();
            if (dbitem != null)
            {
                dbitem.Category = taskItem.Category;
                dbitem.Description = taskItem.Description;
                dbitem.DueDate = taskItem.DueDate;
                dbitem.Mark = taskItem.Mark;
                dbitem.Name = taskItem.Name;
                dbitem.ParentId = taskItem.ParentId;
                dbitem.Priority = taskItem.Priority;
                dbitem.ProjectId = taskItem.ProjectId;
                dbitem.LastModified = DateTime.Now;
                dbitem.StartDate = taskItem.StartDate;
                dbitem.TimeEstimated = taskItem.TimeEstimated;
                ModelState.Clear();
                TryValidateModel(dbitem);
                if (ModelState.IsValid)
                {
                    db.Entry(dbitem).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = dbitem.Id });
                }
            }


            ViewBag.ProjectId = new SelectList(db.Project, "Id", "Name", taskItem.ProjectId);
            ViewBag.Mark = new SelectList(db.TaskMark, "Id", "Caption", taskItem.Mark);
            ViewBag.ParentId = new SelectList(db.TaskItem.Where(x => x.Active == true), "Id", "Name", taskItem.ParentId);
            ViewBag.Priority = new SelectList(_helper.GetTaskPriority(), "Id", "Name", taskItem.Priority);

            return View(taskItem);
        }

        #endregion

        #region Delete Task
        // GET: TaskItems/Delete/5
        [Authorize(Roles = "admin,taskCRUD")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TaskItem taskItem = db.TaskItem.Find(id);
            if (taskItem == null)
            {
                return HttpNotFound();
            }
            return View(taskItem);
        }

        // POST: TaskItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,taskCRUD")]
        public ActionResult DeleteConfirmed(int id)
        {
            using (db = new ModelContext())
            {
                TaskItem taskItem = db.TaskItem.Where(x => x.Id == id).FirstOrDefault();
                if (taskItem == null)
                {
                    return HttpNotFound();
                }
                taskItem.Active = false;
                db.Entry(taskItem).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Board");
        }

        #endregion

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
