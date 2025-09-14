using Estore.Models;


namespace Estore.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository: IRepository<ShoppingCartModel>
    {
        void Update(ShoppingCartModel cart);
    }
}
