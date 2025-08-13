using Estore.Models;

namespace Estore.DataAccess.Repository.IRepository
{
    public interface IProductRepository: IRepository<ProductModel>
    {
        void Update(ProductModel product);
    }
}
