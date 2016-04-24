using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DbModel;
using MvcToDo.Core.Repository;

namespace MvcToDo.Persistence.Repository
{
    public class CommentsRepository : Repository<Comments>, ICommentsRepository
    {
        public CommentsRepository(ModelContext context) : base(context)
        {
        }
    }
}