using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModel;
using MvcToDo.ModelsView;
using System.Linq.Expressions;

namespace MvcToDo.Core.Repository
{
    public interface IProjectRepository : IRepository<Project>
    {
        /// <summary>
        /// Get a SimpleList collection of projects that are connected to this customer
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IEnumerable<SimpleList> GetProjectsByCustomerId(int id);

        /// <summary>
        /// Get a SimpleList collection of projects that are active or not depending in the arg
        /// </summary>
        /// <param name="active">determine which projects to select based in active propery</param>
        /// <returns></returns>
        IEnumerable<SimpleList> GetAllProjectSimpleList(bool active);

        /// <summary>
        /// Get a SimpleList collection of projects, on which the user has at least a task assigned to them.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IEnumerable<SimpleList> GetUserProjects(string userId);

    }
}
