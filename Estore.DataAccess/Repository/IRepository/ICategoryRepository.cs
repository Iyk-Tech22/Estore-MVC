using Estore.Models;

namespace Estore.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository: IRepository<CategoryModel>
    {
        void Update(CategoryModel category);
    }
}
