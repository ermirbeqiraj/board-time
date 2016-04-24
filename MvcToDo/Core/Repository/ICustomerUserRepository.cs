using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbModel;
namespace MvcToDo.Core.Repository
{
    public interface ICustomerUserRepository : IRepository<CustomerUser>
    {
        int GetCustomerIdByUserId(string id);
    }
}
