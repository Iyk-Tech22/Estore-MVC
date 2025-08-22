using Estore.Models;

namespace Estore.DataAccess.Repository.IRepository
{
    public interface ICompanyRepository: IRepository<CompanyModel>
    {
        void Update(CompanyModel company);
    }
}
