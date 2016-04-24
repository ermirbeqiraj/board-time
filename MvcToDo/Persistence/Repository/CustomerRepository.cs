using DbModel;
using MvcToDo.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcToDo.Persistence.Repository
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ModelContext context) : base(context)
        {
        }
    }
}