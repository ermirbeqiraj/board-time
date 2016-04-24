using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModel;

namespace MvcToDo.Core.Repository
{
    public interface IFilesRepository : IRepository<Files>
    {
        IEnumerable<Files> GetAllFilesByTaskId(int id);
        IEnumerable<Files> GetAllActiveFilesByTaskId(int id);
    }
}
