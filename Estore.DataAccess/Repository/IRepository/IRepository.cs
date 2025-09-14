using System.Linq.Expressions;

namespace Estore.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);
        T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool track = false);
        void Add(T entity);
        void Delete(T entity);
        void DeleteMany(IEnumerable<T> entities);
    }
}
