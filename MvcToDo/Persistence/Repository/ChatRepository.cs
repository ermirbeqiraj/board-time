using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DbModel;
using MvcToDo.Core.Repository;
namespace MvcToDo.Persistence.Repository
{
    public class ChatRepository : Repository<Chat>, IChatRepository
    {
        public ChatRepository(ModelContext context) : base(context)
        {
        }
    }
}