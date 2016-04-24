using DbModel;
using MvcToDo.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcToDo.Persistence.Repository
{
    public class TaskMarkRepository : Repository<TaskMark>, ITaskMarkRepository
    {
        public TaskMarkRepository(ModelContext context) : base(context)
        {
        }
    }
}