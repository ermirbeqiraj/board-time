using DbModel;
using MvcToDo.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcToDo.Persistence.Repository
{
    public class FilesRepository : Repository<Files>, IFilesRepository
    {
        public FilesRepository(ModelContext context) : base(context)
        {
        }

        public IEnumerable<Files> GetAllActiveFilesByTaskId(int id)
        {
            return (from f in _db.Files
                    join tf in _db.TaskFiles on f.Id equals tf.FileId
                    join t in _db.TaskItem on tf.TaskId equals t.Id
                    where t.Id == id && tf.Active && f.Active
                    select f).AsEnumerable();
        }

        public IEnumerable<Files> GetAllFilesByTaskId(int id)
        {
            return (from f in _db.Files
                    join tf in _db.TaskFiles on f.Id equals tf.FileId
                    join t in _db.TaskItem on tf.TaskId equals t.Id
                    where t.Id == id
                    select f).AsEnumerable();
        }
    }
}