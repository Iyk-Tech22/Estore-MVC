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
    public class CategoryRepository : Repository<CategoryModel>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly DbSet<CategoryModel> _dbSet;

        public CategoryRepository(ApplicationDbContext db): base (db)
        {
            _db = db;
            _dbSet = _db.Set<CategoryModel>();
        }

        public void Update(CategoryModel category)
        {
            _dbSet.Update(category);
        }
    }
}
