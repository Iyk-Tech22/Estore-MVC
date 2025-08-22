using Estore.DataAccess.Repository.IRepository;
using Estore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public IActionResult Index()
        {
            IEnumerable<CompanyModel> companies = _unitOfWork.Company.GetAll().ToList();
            return View(companies);
        }

        [HttpGet]
        public IActionResult UpSert(int? companyId)
        {
            CompanyModel company = new CompanyModel();
            if(companyId == null || companyId == 0)
            {
                return View(company);
            }
            company = _unitOfWork.Company.Get(c => c.Id == companyId);
            return View(company);
        }

        [HttpPost]
        public IActionResult UpSert(CompanyModel company)
        {
            if (ModelState.IsValid)
            {
                if(company.Id == 0)
                {
                    _unitOfWork.Company.Add(company);
                }
                else
                {
                    _unitOfWork.Company.Update(company);
                }
                _unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }
            return View(company);
        }


        // APIs CONTROLLERS
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                IEnumerable<CompanyModel> companies = _unitOfWork.Company.GetAll();
                return Json(new { status = "Success", data = companies });
            }
            catch (Exception)
            {

                return Json(new { status = "Error", message = "Something went wrong..." });
            }
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            try
            {
                CompanyModel company = _unitOfWork.Company.Get(c => c.Id == id);
                if (company == null)
                {
                    return Json(new { status = "Error", message = "Could'nt perform delete operation!" });
                }
                _unitOfWork.Company.Delete(company);
                _unitOfWork.Save();
                return Json(new { status = "Success", message = "Company record, delete successfully!" });
            }
            catch (Exception) {
                return Json(new { status = "Error", message = "Something went wrong..." });
            }
        }
    }
}
