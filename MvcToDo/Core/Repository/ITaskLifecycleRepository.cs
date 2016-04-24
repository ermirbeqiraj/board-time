using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModel;
using MvcToDo.ModelsView;

namespace MvcToDo.Core.Repository
{
    public interface ITaskLifecycleRepository : IRepository<TaskLifecycle>
    {
        IEnumerable<TaskMovementsView> GetTaskMovements(int skip, int take);
    }
}
