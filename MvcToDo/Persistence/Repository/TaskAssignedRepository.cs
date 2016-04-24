using DbModel;
using MvcToDo.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcToDo.ModelsView;

namespace MvcToDo.Persistence.Repository
{
    public class TaskAssignedRepository : Repository<TaskAssigned>, ITaskAssignedRepository
    {
        public TaskAssignedRepository(ModelContext context) : base(context)
        {
            
        }

        public IEnumerable<TaskModePerUser> CountTaskCategoryPerUser()
        {
            return (from TaskAssigned in _db.TaskAssigned
                    group new
                    {
                        TaskAssigned,
                        TaskAssigned.TaskItem.TaskCategory
                    }
                    by new
                    {
                        TaskAssigned.AssignedTo,
                        TaskAssigned.TaskItem.TaskCategory.Caption
                    } into g
                    orderby
                      g.Key.AssignedTo
                    select new TaskModePerUser
                    {
                        User = g.Key.AssignedTo,
                        Mode = g.Key.Caption,
                        Count = g.Count()
                    });
        }

        public IEnumerable<TaskModePerUser> CountTaskModePerUser()
        {
            return (from TaskAssigned in _db.TaskAssigned
                    group new
                    {
                        TaskAssigned,
                        TaskAssigned.TaskItem.TaskMark
                    }
                    by new
                    {
                        TaskAssigned.AssignedTo,
                        TaskAssigned.TaskItem.TaskMark.Caption
                    } into g
                    orderby
                      g.Key.AssignedTo
                    select new TaskModePerUser
                    {
                        User = g.Key.AssignedTo,
                        Mode = g.Key.Caption,
                        Count = g.Count()
                    });//.ToList();
        }

        public IEnumerable<Stats> CountTasksPerUser()
        {
            return (_db.TaskAssigned
                            .GroupBy(x => x.AssignedTo)
                            .Select(group => new
                            {
                                User = group.FirstOrDefault().AssignedTo,
                                Count = group.Count()
                            }).ToList()
                            .Select(x => new Stats { Count = x.Count, User = x.User }).ToList());
        }

        public bool UserAccessProject(string userId, int projectId)
        {
            return (from a in _db.TaskAssigned
                          join t in _db.TaskItem on a.TaskId equals t.Id
                          where t.ProjectId == projectId
                                && a.AssignedTo == userId
                                && t.Active
                              && a.Active
                          select t.Id).Any();
        }
    }
}