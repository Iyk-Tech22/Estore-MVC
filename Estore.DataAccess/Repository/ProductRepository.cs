using Estore.DataAccess.Db;
using Estore.DataAccess.Repository.IRepository;
using Estore.Models;
using Microsoft.EntityFrameworkCore;

namespace Estore.DataAccess.Repository
{
    public class ProductRepository: Repository<ProductModel>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly DbSet<ProductModel> dbSet;

        public ProductRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
            dbSet = _db.Set<ProductModel>();
        }

        public void Update(ProductModel product)
        {
            ProductModel? productFromDb = dbSet.FirstOrDefault(p => p.Id == product.Id);
            if(productFromDb != null)
            {
                productFromDb.Title = product.Title;
                productFromDb.Description = product.Description;
                productFromDb.Author = product.Author;
                productFromDb.ISBN = product.ISBN;
                productFromDb.ListPrice = product.ListPrice;
                productFromDb.Price = product.Price;
                productFromDb.Price50 = product.Price50;
                productFromDb.Price100 = product.Price100;
                if(product.ImageUrl != null)
                {
                    productFromDb.ImageUrl = product.ImageUrl;
                }
                dbSet.Update(productFromDb);
            }
        }
       
    }
}
