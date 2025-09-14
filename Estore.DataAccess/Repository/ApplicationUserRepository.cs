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
    public class ApplicationUserRepository: Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly DbSet<ApplicationUser> _dbSet;

        public ApplicationUserRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
            _dbSet = db.Set<ApplicationUser>();
        }
    }
}
