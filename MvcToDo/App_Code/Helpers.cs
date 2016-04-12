using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MvcToDo.Models;
using MvcToDo.ModelsView;


namespace MvcToDo.App_Code
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class Helpers
    {  
        ModelContext db;

        public static void /*async Task*/ AddMessage(string from, string to, string message, DateTime time)
        {
            //using (ModelContext _db = new ModelContext())
            //{
            //    /*
            //     * ok then , this is a little weared, but we have to do so.
            //     * 2  users can have a lot of messages, so we dont want to fill the "Chat" table with useless data.
            //     * reminder : GUID size = 16 bytes, short = 2 bytes
            //     * so we save them once and then we use this combination for further messages,
            //     * if usr_1 == from then the direction of message is from user 1 to user 2
            //     * otherwise the direction is from user 2 to user 1
            //     * In this query, we select the ID of the conversation between these two users, so we can use it to insert a new
            //     * row in the chat table
            //     */
            //    var conversation = _db.Conversation
            //                        .Where(x => (x.Usr_1 == from && x.Usr_2 == to)
            //                                || (x.Usr_1 == to && x.Usr_2 == from))
            //                        .Select(x => new { Id = x.Id, Asc = (x.Usr_1 == from) }).FirstOrDefault();

            //    if (conversation == null)
            //    {// it is the very first message between them, so lets create the conversation row
            //        Conversation c = new Conversation
            //        {
            //            Usr_1 = from,
            //            Usr_2 = to
            //        };
            //        _db.Conversation.Add(c);
            //        _db.SaveChanges();
            //        // c# is really really cool
            //        conversation = new { Id = c.Id, Asc = true };
            //    }

            //    //Chat chat = new Chat { ConversationId = conversation.Id, Created = DateTime.Now, Message = message, OrderedAsc = conversation.Asc };
            //    _db.Chat.Add(new Chat { ConversationId = conversation.Id, Created = time, Message = message, OrderedAsc = conversation.Asc });
            //    await _db.SaveChangesAsync();
            //}
        }

        public bool UserAccessProject(string userId , int projectId)
        {
            db = new ModelContext();

            var result = (from a in db.TaskAssigned
                              join t in db.TaskItem on a.TaskId equals t.Id
                          where t.ProjectId == projectId
                                && a.AssignedTo == userId
                                && t.Active
                              && a.Active
                          select  t.Id).Any();

            return result;
        }

        /// <summary>
        /// Return a List of TaskBoard that belongs to a simple project
        /// </summary>
        /// <param name="ProjectId">The id of the project or null</param>
        /// <returns>List of TaskBoard</returns>
        public List<TaskBoard> GetTaskByProjectId(int projectId, int? categoryId, DateTime? dueDate, string userId = "")
        {
            List<TaskBoard> taskItem = null;
            using (db = new ModelContext())
            {
                if (string.IsNullOrEmpty(userId))
                {
                    taskItem = db.TaskItem
                        .Where(x => x.ProjectId == projectId
                            && x.Active == true
                            && x.TaskMark.Active == true
                            && (categoryId == null || x.Category == categoryId)
                            && (dueDate == null || x.DueDate == dueDate)
                        )
                         .Select(x => new TaskBoard
                         {
                             Id = x.Id.ToString(),
                             Name = x.Name,
                             MarkId = x.Mark,
                             Category = x.TaskCategory.Caption,
                             CssClass = x.TaskCategory.Color
                         }).ToList();
                }
                else
                {
                    taskItem = (from x in db.TaskItem
                                join a in db.TaskAssigned on x.Id equals a.TaskId
                                where x.ProjectId == projectId
                                    && x.Active == true
                                    && x.TaskMark.Active == true
                                    && (categoryId == null || x.Category == categoryId)
                                    && (dueDate == null || x.DueDate == dueDate)
                                    && a.AssignedTo == userId
                                select new TaskBoard
                                {
                                    Id = x.Id.ToString(),
                                    Name = x.Name,
                                    MarkId = x.Mark,
                                    Category = x.TaskCategory.Caption,
                                    CssClass = x.TaskCategory.Color
                                }).ToList();
                }
            }
            return taskItem ?? new List<TaskBoard>();
        }

        /// <summary>
        /// Return a collection of TaskList
        /// </summary>
        /// <param name="projectId">The Id of the project</param>
        /// <param name="categoryId">Task Category</param>
        /// <param name="dueDate">Task Due Date</param>
        /// <param name="userId">Task assigned to user</param>
        /// <returns></returns>
        public List<TaskList> GetTasksForList(int projectId, int? categoryId, DateTime? dueDate, string userId = "")
        {
            db = new ModelContext();
            List<TaskList> taskItem = new List<TaskList>();
            int project = projectId;
            if (string.IsNullOrEmpty(userId))
            { // filter without user
                taskItem = (from x in db.TaskItem
                            where x.ProjectId == project
                                && x.Active == true
                                && (categoryId == null || x.Category == categoryId)
                                && (dueDate == null || x.DueDate == dueDate)
                            select new
                            {
                                Id = x.Id,
                                Name = x.Name,
                                Mode = x.TaskMark.Caption,
                                Priority = x.Priority
                            }).AsEnumerable()
                            .Select(x => new TaskList 
                            {
                                Id = x.Id,
                                Name = x.Name,
                                Mode = x.Mode,
                                Priority = GetTaskPriority().Find(p => p.Id == x.Priority).Name
                            }).ToList();
            }
            else
            { // normal filter
                taskItem = (from x in db.TaskItem
                            join a in db.TaskAssigned on x.Id equals a.TaskId
                            where x.ProjectId == project
                                 && x.Active == true
                                && (categoryId == null || x.Category == categoryId)
                                && (dueDate == null || x.DueDate == dueDate)
                                && a.AssignedTo == userId
                            select new
                            {
                                Id = x.Id,
                                Name = x.Name,
                                Mode = x.TaskMark.Caption,
                                Priority = x.Priority
                            }).AsEnumerable()
                            .Select(x => new TaskList
                            {
                                Id = x.Id,
                                Name = x.Name,
                                Mode = x.Mode,
                                Priority = GetTaskPriority().Find(p => p.Id == x.Priority).Name
                            }).ToList();
            }
            db.Dispose();
            return taskItem;
        }

        /// <summary>
        /// Returns a new TaskItem based on old task that is to be dublicated
        /// Based on DublicateTask properties the comments,users assigned and files are copied to the new task
        /// </summary>
        /// <param name="model">DublicateTask item</param>
        /// <param name="UserId">The author of the old task</param>
        /// <returns>TaskItem</returns>
        public TaskItem GetNewTaskForDublicate(DublicateTask model, string UserId)
        {
            TaskItem newTask = null;
            List<Comments> comments = null;
            List<TaskAssigned> assigned = null;
            List<TaskFiles> files = null;
            using (db = new ModelContext())
            {
                var oldTask = db.TaskItem.Where(x => x.Id == model.TaskId).FirstOrDefault();
                if (oldTask != null)
                {
                    newTask = new TaskItem
                    {
                        Name = model.TaskName,
                        ParentId = oldTask.ParentId,
                        DueDate = oldTask.DueDate,
                        Description = oldTask.Description,
                        Author = UserId,
                        TimeEstimated = oldTask.TimeEstimated,
                        Status = oldTask.Status,
                        ProjectId = oldTask.ProjectId,
                        StartDate = oldTask.StartDate,
                        Priority = oldTask.Priority,
                        Mark = oldTask.Mark,
                        Category = oldTask.Category,
                        Active = true,
                        Created = DateTime.Now,
                        LastModified = DateTime.Now
                    };
                    if (model.IncludeComments)
                    {
                        comments = oldTask.Comments.ToList();
                        foreach (var item in comments)
                            newTask.Comments.Add(new Comments { Active = true, Author = item.Author, Comment = item.Comment, Created = item.Created });
                    }
                    if (model.IncludeFiles)
                    {
                        files = oldTask.TaskFiles.ToList();
                        foreach (var item in files)
                            newTask.TaskFiles.Add(new TaskFiles { Active = true, FileId = item.FileId });
                    }
                    if (model.IncludeUsers)
                    {
                        assigned = oldTask.TaskAssigned.ToList();
                        foreach (var item in assigned)
                            newTask.TaskAssigned.Add(new TaskAssigned { Active = true, AssignedTo = item.AssignedTo, Author = item.Author });
                    }
                }
            }
            return newTask;
        }

        /// <summary>
        /// Task Priority is not represented in the db as a table, instead it's just a task property as number.
        /// Based on the number from 1 to 5 the method will return a key-value list that is more human readable
        /// </summary>
        /// <returns>Key-Value list of TaskPriority</returns>
        public List<DropDownItems> GetTaskPriority()
        {
            var priority = new List<DropDownItems>();
            priority.Add(new DropDownItems { Id = 0, Name = "Free Time" });
            priority.Add(new DropDownItems { Id = 1, Name = "Low" });
            priority.Add(new DropDownItems { Id = 2, Name = "Below Normal" });
            priority.Add(new DropDownItems { Id = 3, Name = "Normal" });
            priority.Add(new DropDownItems { Id = 4, Name = "Above Normal" });
            priority.Add(new DropDownItems { Id = 5, Name = "High" });
            return priority;
        }

        public List<DropDownItems> GetCategoryColor()
        {
            var category = new List<DropDownItems>();
            category.Add(new DropDownItems { Id = 1, Name = "label label-default" });
            category.Add(new DropDownItems { Id = 2, Name = "label label-primary" });
            category.Add(new DropDownItems { Id = 3, Name = "label label-success" });
            category.Add(new DropDownItems { Id = 4, Name = "label label-info" });
            category.Add(new DropDownItems { Id = 5, Name = "label label-warning" });
            category.Add(new DropDownItems { Id = 6, Name = "label label-danger" });
            return category;
        }

        private string GetTopString(string args)
        {
            return args.Substring(0, Math.Min(args.Length, 20));
        }
    }
}