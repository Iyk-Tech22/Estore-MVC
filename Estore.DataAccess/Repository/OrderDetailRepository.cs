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
    public class OrderDetailRepository: Repository<OrderDetailModel>, IOrderDetailRepository
    {
        private readonly DbSet<OrderDetailModel> _dbSet;

        public OrderDetailRepository (ApplicationDbContext db): base(db)
        {
            _dbSet = db.Set<OrderDetailModel>();
        }

        public void Update(OrderDetailModel orderDetail)
        {
            this._dbSet.Update(orderDetail);
        }
    }
}
