using MvcToDo.Core;
using MvcToDo.Core.Repository;
using MvcToDo.Persistence.Repository;
using System;
using System.Runtime.InteropServices;

namespace MvcToDo.Persistence
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        public IChatRepository Chat
        {
            get; private set;
        }

        public ICommentsRepository Comments
        {
            get; private set;
        }

        public IConversationRepository Conversation
        {
            get;private set;
        }

        public ICustomerRepository Customer
        {
            get; private set;
        }

        public ICustomerUserRepository CustomerUser
        {
            get; private set;
        }

        public IFilesRepository Files
        {
            get; private set;
        }

        public IProjectRepository Project
        {
            get; private set;
        }

        public ITaskAssignedRepository TaskAssigned
        {
            get; private set;
        }

        public ITaskCategoryRepository TaskCategory
        {
            get;private set;
        }

        public ITaskFilesRepository TaskFiles
        {
            get;private set;
        }

        public ITaskItemRepository TaskItem
        {
            get;private set;
        }

        public ITaskLifecycleRepository TaskLifecycle
        {
            get;private set;
        }

        public ITaskMarkRepository TaskMark
        {
            get; private set;
        }

        ModelContext _db;

        public UnitOfWork(ModelContext context)
        {
            _db = context;

            Chat = new ChatRepository(_db);
            Comments = new CommentsRepository(_db);
            Conversation = new ConversationRepository(_db);
            Customer = new CustomerRepository(_db);
            CustomerUser = new CustomerUserRepository(_db);
            Files = new FilesRepository(_db);
            Project = new ProjectRepository(_db);
            TaskAssigned = new TaskAssignedRepository(_db);
            TaskCategory = new TaskCategoryRepository(_db);
            TaskFiles = new TaskFilesRepository(_db);
            TaskItem = new TaskItemRepository(_db);
            TaskLifecycle = new TaskLifecycleRepository(_db);
            TaskMark = new TaskMarkRepository(_db);
        }

        public int Persist()
        {
            return _db.SaveChanges();
        }

        #region Dispose
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);
        ~UnitOfWork()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_db != null)
                {
                    _db.Dispose();
                    _db = null;
                }
            }
            if (nativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(nativeResource);
                nativeResource = IntPtr.Zero;
            }
        }
        #endregion

    }
}