using Microsoft.AspNetCore.Mvc;

namespace EstoreWeb.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
