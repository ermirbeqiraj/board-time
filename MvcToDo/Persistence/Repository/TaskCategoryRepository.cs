using DbModel;
using MvcToDo.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcToDo.Persistence.Repository
{
    public class TaskCategoryRepository : Repository<TaskCategory>, ITaskCategoryRepository
    {
        public TaskCategoryRepository(ModelContext context) : base(context)
        {
        }
    }
}