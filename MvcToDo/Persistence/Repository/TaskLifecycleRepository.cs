using DbModel;
using MvcToDo.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcToDo.ModelsView;

namespace MvcToDo.Persistence.Repository
{
    public class TaskLifecycleRepository : Repository<TaskLifecycle>, ITaskLifecycleRepository
    {
        public TaskLifecycleRepository(ModelContext context) : base(context)
        {
        }

        public IEnumerable<TaskMovementsView> GetTaskMovements(int skip, int take)
        {
           var result = (from stat in _db.TaskLifecycle
                      join task in _db.TaskItem on stat.TaskId equals task.Id
                      join mark in _db.TaskMark on stat.MarkFromId equals mark.Id
                      join mark2 in _db.TaskMark on stat.MarkToId equals mark2.Id
                      orderby stat.Created descending
                      select new
                      {
                          ProjectId = task.ProjectId,
                          TaskId = task.Id,
                          Actor = stat.Actor,
                          TaskName = task.Name,
                          FromMode = mark.Caption,
                          ToMode = mark2.Caption,
                          Created = stat.Created
                      }
                        ).Skip(skip)
                        .Take(take)
                        .AsEnumerable()
                        .Select(x => new TaskMovementsView
                        {
                            ProjectId = x.ProjectId,
                            TaskId = x.TaskId,
                            Actor = x.Actor,
                            Created = x.Created.ToString("dd-MMM-yy HH:mm"),
                            FromMode = x.FromMode,
                            ToMode = x.ToMode,
                            TaskName = x.TaskName
                        }).ToList();
            return result ?? new List<TaskMovementsView>();
        }
    }
}