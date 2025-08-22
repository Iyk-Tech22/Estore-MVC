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
    public class CompanyRepository: Repository<CompanyModel>, ICompanyRepository
    {
        private readonly DbSet<CompanyModel> dbSet;
        public CompanyRepository(ApplicationDbContext db) : base(db) {
            dbSet = db.Set<CompanyModel>();
        }

        public void Update(CompanyModel company)
        {
           this.dbSet.Update(company);
        }
    }
}
