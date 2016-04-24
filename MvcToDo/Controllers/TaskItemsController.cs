using DbModel;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MvcToDo.App_Code;
using MvcToDo.ModelsView;
using MvcToDo.Persistence;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MvcToDo.Controllers
{
    [Authorize]
    public class TaskItemsController : Controller
    {
        Helpers _helper = new Helpers();
        ApplicationUserManager _userManager;

        UnitOfWork _repo;
        public TaskItemsController()
        {
            _repo = new UnitOfWork(new ModelContext());
        }

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
                if (User.IsInRole("admin") || _repo.TaskAssigned.UserAccessProject(User.Identity.GetUserId(), projectId))
                {
                    //taskItem = _helper.GetTasksForList(projectId, CategoryId, DueDate, UserId);
                    taskItem = _repo.TaskItem.GetTasksForList(projectId, CategoryId, DueDate, UserId).ToList();
                    taskItem.ForEach(x => x.Priority = _helper.GetTaskPriority().Where(p => p.Id.ToString() == x.Priority).FirstOrDefault().Name);

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
            var Categories = _repo.TaskCategory.GetAll().Select(x => new { Id = x.Id, Name = x.Caption })
                .AsEnumerable().Select(x => new SimpleList { Id = x.Id, Name = x.Name }).ToList();
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
            if (int.TryParse(route.ToString(), out projectId) && (User.IsInRole("admin") || _repo.TaskAssigned.UserAccessProject(User.Identity.GetUserId(), projectId)))
            {
                viewItems.BoardColumns = _repo.TaskMark.Find(x => x.Active).ToList();
                viewItems.Board = _repo.TaskItem.GetTasksForBoard(projectId, CategoryId, DueDate, UserId).ToList();
                var Categories = _repo.TaskCategory.GetAll().Select(x => new { Id = x.Id, Name = x.Caption })
                    .AsEnumerable().Select(x => new SimpleList { Id = x.Id, Name = x.Name }).ToList();
                var UsersDrop = UserManager.Users.Select(x => new StringList { Id = x.Id, Name = (x.FirstName + " " + x.LastName) }).ToList();
                viewItems.TCategories = new SelectList(Categories, "Id", "Name", CategoryId);
                viewItems.TUsers = new SelectList(UsersDrop, "Id", "Name", UserId);
                return View(viewItems);
            }
            else if(int.TryParse(route.ToString(), out projectId) && (User.IsInRole("customer")))
            {
                string currentUserId = User.Identity.GetUserId();
                int customerId = _repo.CustomerUser.GetCustomerIdByUserId(currentUserId);
                var projects = _repo.Project.Find(x => x.CustomerId == customerId && x.Active == true).Select(x => x.Id).ToList();
                if (projects.Contains(projectId))
                {
                    viewItems.BoardColumns = _repo.TaskMark.Find(x => x.Active).ToList();
                    viewItems.Board = _repo.TaskItem.GetTasksForBoard(projectId, CategoryId, DueDate, UserId).ToList();
                    var Categories = _repo.TaskCategory.GetAll().Select(x => new { Id = x.Id, Name = x.Caption })
                    .AsEnumerable().Select(x => new SimpleList { Id = x.Id, Name = x.Name }).ToList();

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
                byte _markId, _from;
                if (int.TryParse(id, out _taskId) && byte.TryParse(mark, out _markId) && byte.TryParse(from, out _from) && (mark != from))
                {
                    /*
                     * a task can be moved between columns just by the admin or by a user that is assigned to that task
                     */
                    string currentUser = User.Identity.GetUserId();
                    var res = (_repo.TaskAssigned.Find(x => x.TaskId == _taskId && x.AssignedTo == currentUser).FirstOrDefault() != null);
                    if (res || User.IsInRole("admin"))
                    {
                        var item = _repo.TaskItem.GetSingle(x => x.Id == _taskId);
                        if (item != null)
                        {
                            item.Mark = _markId;
                            _repo.TaskItem.Update(item);
                            _repo.Persist();
                            result = "1";

                            TaskLifecycle taskLifecycle = new TaskLifecycle { Actor = currentUser, TaskId = _taskId, Created = DateTime.Now, MarkFromId = _from, MarkToId = _markId };
                            _repo.TaskLifecycle.Add(taskLifecycle);
                            _repo.Persist();
                        }
                        else
                        {
                            result = "Item could not be found!";
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
            TaskItem taskItem = _repo.TaskItem.Get(id.Value);
            if (taskItem == null)
            {
                return HttpNotFound();
            }

            // ensure user has the rights to access this task
            #region access
            int projectId = taskItem.ProjectId;
            string userId = User.Identity.GetUserId();
            if (!User.IsInRole("admin") && !_repo.TaskAssigned.UserAccessProject(userId, projectId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            #endregion
            // get comments
            var comments = _repo.Comments.Find(x => x.TaskId == taskItem.Id).ToList();
            foreach (var x in comments)
            {
                string currId = x.Author;
                var userCom = UserManager.Users.Where(u => u.Id == currId).FirstOrDefault();
                x.Author = userCom != null ? (userCom.FirstName + " " + userCom.LastName) : "Not Found!";
                x.Comment = HttpUtility.HtmlDecode(x.Comment);
            }

            //get files
            var atFiles = _repo.Files.GetAllActiveFilesByTaskId(taskItem.Id).ToList();
            // get assignedTo
            string[] users = _repo.TaskAssigned.Find(x => x.TaskId == id).Select(x => x.AssignedTo).ToArray();
            var atUsers = UserManager.Users.Where(x => users.Contains(x.Id)).ToList();
            string allUsers = "";
            foreach (var x in atUsers)
            {
                allUsers += x.FirstName + " " + x.LastName + " ,";
            }

            TaskItemView currTask = new TaskItemView(comments, allUsers, atFiles, taskItem);
            currTask.TaskMark = _repo.TaskMark.Get(taskItem.Mark).Caption;
            currTask.ProjectName = _repo.Project.Get(taskItem.ProjectId).Name;
            string curraId = taskItem.Author;
            var currUser = UserManager.Users.Where(u => u.Id == curraId).FirstOrDefault();
            currTask.Author = currUser != null ? (currUser.FirstName + " " + currUser.LastName) : "Not Found!";

            // parent task:
            ViewBag.ParentTask = taskItem.ParentId == null ? "-" : _repo.TaskItem.Find(x => x.ParentId == taskItem.ParentId).FirstOrDefault().Name;
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
            TaskItem taskItem = _repo.TaskItem.Get(Id);
            if (taskItem == null)
                throw new HttpException(404, "File not found");

            // ensure user has the rights to access this task
            int projectId = taskItem.ProjectId;
            string userId = User.Identity.GetUserId();
            if (!User.IsInRole("admin") && !_repo.TaskAssigned.UserAccessProject(userId,projectId))
            {
                throw new HttpException(403, "not authorized results!");
            }
            #endregion

            var item = _repo.Files.Get(FileId);
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
            TaskItem taskItem = _repo.TaskItem.Get(Id);
            if (taskItem == null)
                return HttpNotFound();

            // ensure user has the rights to access this task
            int projectId = taskItem.ProjectId;
            string userId = User.Identity.GetUserId();
            if (!User.IsInRole("admin") && !_repo.TaskAssigned.UserAccessProject(userId, projectId))
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
                _repo.Comments.Add(item);
                _repo.Persist();
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
            TaskItem taskItem = _repo.TaskItem.Get(Id);
            if (taskItem == null)
                return HttpNotFound();

            // ensure user has the rights to access this task
            int projectId = taskItem.ProjectId;
            string userId = User.Identity.GetUserId();
            if (!User.IsInRole("admin") && !_repo.TaskAssigned.UserAccessProject(userId, projectId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            #endregion

            // task id is nessesary for getting files for the right task
            var items = _repo.Files.GetAllActiveFilesByTaskId(Id).ToList();
            ViewBag.TaskId = Id;
            return View(items);
        }

        // get TaskItem/AddFile/5
        [Authorize(Roles = "admin,taskAddFiles")]
        public ActionResult AddFile(int id)
        {
            //GET verb to the view for adding a new file to the task
            var task = _repo.TaskItem.Get(id);
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
                // get last part
                int taskId = int.Parse(items[items.Length - 1]);
                result = _repo.Files.GetAllActiveFilesByTaskId(taskId).Select(x => new SimpleList { Id = x.Id,Name = x.FileName }).ToList();
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin,taskAddFiles")]
        public ActionResult CopyFile([Bind(Include = "fileId,toTaskId")] int fileId, int toTaskId)
        {
            var file = _repo.Files.Get(fileId);
            if (file == null)
            {
                return HttpNotFound();
            }

            TaskFiles tf = new TaskFiles { FileId = file.Id, TaskId = toTaskId, Active = true };
            _repo.TaskFiles.Add(tf);
            _repo.Persist();

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
                        _repo.Files.Add(item);
                        _repo.Persist();
                        var tfiles = new TaskFiles { TaskId = Id, FileId = item.Id, Active = true };
                        //GetId
                        //Add TaskFiles
                        _repo.TaskFiles.Add(tfiles);
                        _repo.Persist();
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
            var item = _repo.Files.Get(id.Value);
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
            var item = _repo.Files.Get(id);
            if (item != null)
            {
                var taskFiles = _repo.TaskFiles.Find(x => x.FileId == item.Id).ToList();
                _repo.TaskFiles.RemoveRange(taskFiles);
                _repo.Files.Remove(item);
                return RedirectToAction("Files", new { id = id });
            }
            else
            {
                return HttpNotFound();
            }
        }

        #endregion

        #region Users
        // Get : TaskItems/Users/1
        [Authorize(Roles = "admin,taskAttachUser")]
        public ActionResult Users(int Id)
        {
            // get assignedTo
            string[] assignedUsers = _repo.TaskAssigned.Find(x => x.TaskId == Id).Select(x => x.AssignedTo).ToArray();
            var atUsers = UserManager.Users.Where(x => assignedUsers.Contains(x.Id))
                .Select(x => new UserChecks() { Id = x.Id, Name = x.UserName, Checked = true }).ToList();

            var notAssigned = UserManager.Users.Where(x => !assignedUsers.Contains(x.Id))
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
                var itemstodel = _repo.TaskAssigned.Find( x => x.TaskId == Id).ToList();
                _repo.TaskAssigned.RemoveRange(itemstodel);
                
                foreach (var item in UsersToAdd)
                {
                    taskAssigned = new TaskAssigned
                    {
                        Active = true,
                        AssignedTo = item,
                        Author = currentAuthor,
                        TaskId = Id
                    };
                    _repo.TaskAssigned.Add(taskAssigned);
                    _repo.Persist();
                }
                return RedirectToAction("Details", new { id = Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Exception", ex.Message);
            }

            ModelState.AddModelError("", "Insertion Failed!");

            var users = UserManager.Users.Select(x => new UserChecks() { Id = x.Id, Name = x.UserName }).ToList();
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

            var parents = _repo.TaskItem.Find(x => x.Active == true);
            ViewBag.ProjectCollection = new SelectList(_repo.Project.GetAllProjectSimpleList(true), "Id", "Name", projectId);
            ViewBag.Mark = new SelectList(_repo.TaskMark.Find(x => x.Active), "Id", "Caption");
            ViewBag.ParentCollection = new SelectList(parents, "Id", "Name", (Parent.HasValue ? Parent.Value : -1));
            ViewBag.PriorityBag = new SelectList(_helper.GetTaskPriority(), "Id", "Name","3");
            ViewBag.Taskcategory = new SelectList(_repo.TaskCategory.GetAll(), "Id", "Caption");
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
                _repo.TaskItem.Add(taskItem);
                _repo.Persist();
                return RedirectToAction("Details", new { id = taskItem.Id});
            }

            var parents = _repo.TaskItem.Find(x => x.Active == true);
            ViewBag.ProjectCollection = new SelectList(_repo.Project.GetAllProjectSimpleList(true), "Id", "Name", projectId);
            ViewBag.Mark = new SelectList(_repo.TaskMark.Find(x => x.Active), "Id", "Caption");
            ViewBag.ParentCollection = new SelectList(parents, "Id", "Name", taskItem.ParentId);
            ViewBag.PriorityBag = new SelectList(_helper.GetTaskPriority(), "Id", "Name", "3");
            ViewBag.Taskcategory = new SelectList(_repo.TaskCategory.GetAll(), "Id", "Caption");

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
            var taskItem = _repo.TaskItem.Get(id.Value);
            if (taskItem != null)
            {
                modelItem.TaskName = taskItem.Name + " (Copy)";
            }
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
                var newTask = _repo.TaskItem.GetNewTaskForDublicate(model, User.Identity.GetUserId());
                if (newTask != null)
                {
                    _repo.TaskItem.Add(newTask);
                    _repo.Persist();
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
            TaskItem taskItem = _repo.TaskItem.Get(id.Value);
            if (taskItem == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProjectId = new SelectList(_repo.Project.GetAllProjectSimpleList(true), "Id", "Name", taskItem.ProjectId);
            ViewBag.Mark = new SelectList(_repo.TaskMark.Find(x => x.Active), "Id", "Caption", taskItem.Mark);
            ViewBag.ParentId = new SelectList(_repo.TaskItem.Find(x => x.Active == true), "Id", "Name", taskItem.ParentId ?? 0);
            ViewBag.PriorityBag = new SelectList(_helper.GetTaskPriority(), "Id", "Name", taskItem.Priority);
            ViewBag.DropCat = new SelectList(_repo.TaskCategory.GetAll(), "Id", "Caption", taskItem.Category);

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
            var dbitem = _repo.TaskItem.GetSingle(x => x.Id == taskItem.Id);
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
                    _repo.TaskItem.Update(dbitem);
                    _repo.Persist();
                    return RedirectToAction("Details", new { id = dbitem.Id });
                }
            }


            ViewBag.ProjectId = new SelectList(_repo.Project.GetAllProjectSimpleList(true), "Id", "Name", taskItem.ProjectId);
            ViewBag.Mark = new SelectList(_repo.TaskMark.Find(x => x.Active), "Id", "Caption", taskItem.Mark);
            ViewBag.ParentId = new SelectList(_repo.TaskItem.Find(x => x.Active == true), "Id", "Name", taskItem.ParentId);
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
            TaskItem taskItem = _repo.TaskItem.Get(id.Value);
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
            TaskItem taskItem = _repo.TaskItem.Get(id);
            if (taskItem == null)
            {
                return HttpNotFound();
            }
            taskItem.Active = false;
            _repo.TaskItem.Update(taskItem);
            _repo.Persist();
            return RedirectToAction("Board");
        }

        #endregion

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
