using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModel;
using MvcToDo.ModelsView;

namespace MvcToDo.Core.Repository
{
    public interface ITaskAssignedRepository : IRepository<TaskAssigned>
    {
        bool UserAccessProject(string userId, int projectId);
        /// <summary>
        /// use this to retrieve a collection of users and number of tasks assigned to each one of them.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Stats> CountTasksPerUser();
        /// <summary>
        /// use this to retrieve a collection of users and number of tasks assigned to each one of them, grouped by task mode.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TaskModePerUser> CountTaskModePerUser();
        /// <summary>
        /// use this to retrieve a collection of users and number of tasks assigned to each one of them, grouped by task category.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TaskModePerUser> CountTaskCategoryPerUser();

    }
}
