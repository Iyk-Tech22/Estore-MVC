using Estore.DataAccess.Repository.IRepository;
using Estore.Models;
using Estore.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EstoreWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnv;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnv)
        {
            _unitOfWork = unitOfWork;
            _webHostEnv = webHostEnv;
        }
        public IActionResult Index()
        {
            List<ProductModel> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return View(products);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(
                    c => new SelectListItem() { Text = c.Name, Value = c.Id.ToString() }
                ),
                Product = new ProductModel()
            };

            if(id == null || id == 0)
            {
                return View(productVM);
            }
            productVM.Product = _unitOfWork.Product.Get(p => p.Id == id);
            return View(productVM);
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            string wwwRootHost = _webHostEnv.WebRootPath;
            string oldImagePath = wwwRootHost + productVM.Product.ImageUrl;

            if (ModelState.IsValid)
            {
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = wwwRootHost + @"\images\product";

                    if (string.IsNullOrEmpty(productVM.Product.ImageUrl) == false)
                    {
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                 }
                else
                {
                    productVM.Product.ImageUrl = "";
                }

                if(productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }

                    _unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(c => new SelectListItem()
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });
                return View(productVM);
            }
        }

        // APIs Endpoints
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                List<ProductModel> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
                return Json(new { data = products });
            }
            catch (Exception)
            {

                return Json(new { status = "Error", message = "Something went wrong..." });
            }
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            try
            {
                ProductModel product = _unitOfWork.Product.Get(p => p.Id == id);

                if (product == null)
                {
                    return Json(new { error = "Error", message = "Error while deleting..." });
                }

                string wwwRootHost = _webHostEnv.WebRootPath;
                string oldImagePath = wwwRootHost + product.ImageUrl;

                if (string.IsNullOrEmpty(product.ImageUrl) == false)
                {
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                _unitOfWork.Product.Delete(product);
                _unitOfWork.Save();

                return Json(new { status = "Success", message = "Delete Successful" });
            }
            catch (Exception)
            {

                return Json(new { status = "Error", message = "Somethin went wrong..." });
            }
        }
    }
    
}
