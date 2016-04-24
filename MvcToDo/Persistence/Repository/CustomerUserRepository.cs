using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DbModel;
using MvcToDo.Core.Repository;

namespace MvcToDo.Persistence.Repository
{
    public class CustomerUserRepository : Repository<CustomerUser>, ICustomerUserRepository
    {
        public CustomerUserRepository(ModelContext context) : base(context)
        {
        }

        public int GetCustomerIdByUserId(string id)
        {
            return _db.CustomerUser.Where(x => x.UserId == id).Select(x => x.CustomerId).FirstOrDefault();
        }
    }
}