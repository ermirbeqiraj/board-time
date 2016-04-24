using DbModel;
using MvcToDo.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcToDo.ModelsView;
using System.Linq.Expressions;

namespace MvcToDo.Persistence.Repository
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        public ProjectRepository(ModelContext context) : base(context)
        {
        }

        public IEnumerable<SimpleList> GetAllProjectSimpleList(bool active)
        {
            return ((from p in _db.Project
                    where p.Active == active
                    select new
                    {
                        Id = p.Id,
                        Name = p.Name
                    }).Distinct()
                        .AsEnumerable()
                        .Select(x => new SimpleList { Id = x.Id, Name = x.Name }).ToList());
        }

        public IEnumerable<SimpleList> GetProjectsByCustomerId(int id)
        {
            return (_db.Project
                    .Where(x => x.CustomerId == id && x.Active == true)
                    .Select(x => new { Id = x.Id, Name = x.Name })
                    .AsEnumerable().Select(x => new SimpleList { Id = x.Id, Name = x.Name }).ToList());
        }

        public IEnumerable<SimpleList> GetUserProjects(string userId)
        {
            return (
                (from p in _db.Project
                 join t in _db.TaskItem on p.Id equals t.ProjectId
                 join a in _db.TaskAssigned on t.Id equals a.TaskId
                 where a.AssignedTo == userId
                     && a.Active
                     && p.Active
                     && t.Active
                 select new
                 {
                     Id = p.Id,
                     Name = p.Name
                 }).Distinct()
                        .AsEnumerable()
                        .Select(x => new SimpleList { Id = x.Id, Name = x.Name }).ToList()
                );
        }
    }
}