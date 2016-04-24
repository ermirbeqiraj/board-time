using DbModel;
using MvcToDo.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcToDo.Persistence.Repository
{
    public class TaskFilesRepository : Repository<TaskFiles>, ITaskFilesRepository
    {
        public TaskFilesRepository(ModelContext context) : base(context)
        {
        }
    }
}