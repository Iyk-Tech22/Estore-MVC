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
    public class ShoppingCartRepository: Repository<ShoppingCartModel>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly DbSet<ShoppingCartModel> _dbSet;

        public ShoppingCartRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
            _dbSet = db.Set<ShoppingCartModel>();
        }

        public void Update(ShoppingCartModel cartItem)
        {
            this._dbSet.Update(cartItem);
        }
    }
}
