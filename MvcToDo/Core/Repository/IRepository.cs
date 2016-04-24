using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MvcToDo.Core.Repository
{
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Get element by id
        /// </summary>
        /// <param name="id">Id of element</param>
        /// <returns></returns>
        T Get(int id);
        T GetSingle(Expression<Func<T, bool>> predicate);
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);

        void Add(T item);
        void AddRange(IEnumerable<T> items);
        void Remove(T item);
        void RemoveRange(IEnumerable<T> items);
        void Update(T item);
    }
}
