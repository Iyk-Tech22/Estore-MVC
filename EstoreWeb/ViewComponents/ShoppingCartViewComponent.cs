using Estore.DataAccess.Repository.IRepository;
using Estore.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EstoreWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                var cartsCount = HttpContext.Session.GetInt32(SD.SessionCart);
                if(cartsCount == null)
                {
                    var result = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value).Count();
                    HttpContext.Session.SetInt32(SD.SessionCart, result);
                    cartsCount = result;
                }
                return View(cartsCount);
            }
            HttpContext.Session.Clear();
            return View(0);
        }

    }
}
