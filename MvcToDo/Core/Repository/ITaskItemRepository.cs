using System;
using System.Collections.Generic;
using DbModel;
using MvcToDo.ModelsView;

namespace MvcToDo.Core.Repository
{
    public interface ITaskItemRepository : IRepository<TaskItem>
    {
        /// <summary>
        /// Return a collection of TaskList
        /// </summary>
        /// <param name="projectId">The Id of the project</param>
        /// <param name="categoryId">Task Category</param>
        /// <param name="dueDate">Task Due Date</param>
        /// <param name="userId">Task assigned to user</param>
        /// <returns></returns>
        IEnumerable<TaskList> GetTasksForList(int projectId, int? categoryId, DateTime? dueDate, string userId = "");

        /// <summary>
        /// Return a List of TaskBoard that belongs to a simple project
        /// </summary>
        /// <param name="ProjectId">The id of the project or null</param>
        /// <returns>List of TaskBoard</returns>
        IEnumerable<TaskBoard> GetTasksForBoard(int projectId, int? categoryId, DateTime? dueDate, string userId = "");

        /// <summary>
        /// Returns a new TaskItem based on old task that is to be dublicated
        /// Based on DublicateTask properties the comments,users assigned and files are copied to the new task
        /// </summary>
        /// <param name="model">DublicateTask item</param>
        /// <param name="UserId">The author of the old task</param>
        /// <returns>TaskItem</returns>
        TaskItem GetNewTaskForDublicate(DublicateTask model, string UserId);

    }
}
