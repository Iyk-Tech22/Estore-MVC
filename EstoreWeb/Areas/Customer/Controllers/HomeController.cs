using System.Diagnostics;
using Estore.DataAccess.Repository.IRepository;
using Estore.Models;
using Microsoft.AspNetCore.Mvc;

namespace EstoreWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<ProductModel> products = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return View(products);
        }

        public IActionResult Details(int? productId)
        {
            ProductModel product = _unitOfWork.Product.Get(p => p.Id == productId, includeProperties: "Category");
            if(product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
