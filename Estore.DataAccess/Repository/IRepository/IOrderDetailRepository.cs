using Estore.Models;

namespace Estore.DataAccess.Repository.IRepository
{
    public interface IOrderDetailRepository: IRepository<OrderDetailModel>
    {
        void Update(OrderDetailModel orderDetail);
    }
}
