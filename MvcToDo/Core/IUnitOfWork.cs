using MvcToDo.Core.Repository;
using System;
namespace MvcToDo.Core
{
    public interface IUnitOfWork : IDisposable
    {
        IChatRepository Chat { get; }
        ICommentsRepository Comments { get; }
        IConversationRepository Conversation { get; }
        ICustomerRepository Customer { get; }
        ICustomerUserRepository CustomerUser { get; }
        IFilesRepository Files { get; }
        IProjectRepository Project { get; }
        ITaskAssignedRepository TaskAssigned { get; }
        ITaskCategoryRepository TaskCategory { get; }
        ITaskFilesRepository TaskFiles { get; }
        ITaskItemRepository TaskItem { get; }
        ITaskLifecycleRepository TaskLifecycle { get; }
        ITaskMarkRepository TaskMark { get; }
        
        /// <summary>
        /// Persist the data into db.
        /// </summary>
        /// <returns>int representing status for this unit of work</returns>
        int Persist();
    }
}
