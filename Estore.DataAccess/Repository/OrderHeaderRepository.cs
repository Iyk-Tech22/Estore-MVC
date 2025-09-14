using Estore.DataAccess.Db;
using Estore.DataAccess.Repository.IRepository;
using Estore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Estore.DataAccess.Repository
{
    public class OrderHeaderRepository: Repository<OrderHeaderModel>, IOrderHeaderRepository
    {
        private readonly DbSet<OrderHeaderModel> _dbSet;

        public OrderHeaderRepository(ApplicationDbContext db): base(db)
        {
            _dbSet = db.Set<OrderHeaderModel>();
        }

        public void Update(OrderHeaderModel orderHeader)
        {
            this._dbSet.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderHeaderFromDB = _dbSet.FirstOrDefault(x => x.Id == id);
            if (orderHeaderFromDB != null)
            {
                orderHeaderFromDB.OrderStatus = orderStatus;
                if(string.IsNullOrEmpty(paymentStatus) == false)
                {
                    orderHeaderFromDB.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderHeaderFromDb = _dbSet.FirstOrDefault(x => x.Id == id);
            if (string.IsNullOrEmpty(sessionId) == false)
            {
                orderHeaderFromDb.SessionId = sessionId;
            }

            if (string.IsNullOrEmpty(paymentIntentId) == false)
            {
                orderHeaderFromDb.PaymentIntentId = paymentIntentId;
                orderHeaderFromDb.PaymentDate = DateTime.Now;
            }
        }
    }
}
