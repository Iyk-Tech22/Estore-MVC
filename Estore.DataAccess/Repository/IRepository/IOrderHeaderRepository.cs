using Estore.Models;

namespace Estore.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository: IRepository<OrderHeaderModel>
    {
        void Update(OrderHeaderModel orderHeader);
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId);
    }
}
