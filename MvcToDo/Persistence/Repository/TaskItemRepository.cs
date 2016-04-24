using DbModel;
using MvcToDo.Core.Repository;
using MvcToDo.ModelsView;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcToDo.Persistence.Repository
{
    public class TaskItemRepository : Repository<TaskItem>, ITaskItemRepository
    {
        public TaskItemRepository(ModelContext context) : base(context)
        {
        }

        public TaskItem GetNewTaskForDublicate(DublicateTask model, string UserId)
        {
            TaskItem newTask = null;
            List<Comments> comments = null;
            List<TaskAssigned> assigned = null;
            List<TaskFiles> files = null;
            var oldTask = _db.TaskItem.Where(x => x.Id == model.TaskId).FirstOrDefault();
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

            return newTask;
        }

        public IEnumerable<TaskBoard> GetTasksForBoard(int projectId, int? categoryId, DateTime? dueDate, string userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                return (_db.TaskItem
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
                       })).AsEnumerable();
            }
            else
            {
                return (from x in _db.TaskItem
                        join a in _db.TaskAssigned on x.Id equals a.TaskId
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
                        }).AsEnumerable();
            }
        }

        public IEnumerable<TaskList> GetTasksForList(int projectId, int? categoryId, DateTime? dueDate, string userId = "")
        {
            List<TaskList> taskItems;
            if (string.IsNullOrEmpty(userId))
            { // filter without user
                taskItems = (from x in _db.TaskItem
                             where x.ProjectId == projectId
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
                                 Priority = x.Priority.ToString()
                             }).ToList();
            }
            else
            { // normal filter
                taskItems = (from x in _db.TaskItem
                             join a in _db.TaskAssigned on x.Id equals a.TaskId
                             where x.ProjectId == projectId
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
                                Priority = x.Priority.ToString()
                            }).ToList();
            }
            return taskItems;
        }
    }
}