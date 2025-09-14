using Estore.DataAccess.Repository.IRepository;
using Estore.Models;
using Estore.Models.ViewModels;
using Estore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace EstoreWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userCarts = _unitOfWork.ShoppingCart.GetAll(cart => cart.ApplicationUserId == userId, "ApplicationUser,Product").ToList();
            double totalOrders = 0;

            foreach (var userCart in userCarts)
            {
                totalOrders += GetPriceBaseOnQuantity(userCart) * userCart.Quantity;
            }

            ShoppingCartVM = new()
            {
                Carts = userCarts,
                OrderHeader = new()
                {
                    TotalOrder = totalOrders
                },
            };
            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int id)
        {
            ShoppingCartModel cartFromDb = _unitOfWork.ShoppingCart.Get(c => c.Id == id);
            cartFromDb.Quantity += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int id)
        {
            ShoppingCartModel cartFromDb = _unitOfWork.ShoppingCart.Get(c => c.Id == id, track:true);
            if(cartFromDb.Quantity <= 1)
            {
                _unitOfWork.ShoppingCart.Delete(cartFromDb);
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
                _unitOfWork.Save();
               
            }
            else
            {
                cartFromDb.Quantity -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            }
                 
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int id)
        {
            ShoppingCartModel cartFromDb = _unitOfWork.ShoppingCart.Get(c => c.Id == id, track:true);
            _unitOfWork.ShoppingCart.Delete(cartFromDb);
            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM shoppingCartVM = new()
            {
                Carts = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == userId, "Product"),
                OrderHeader = new()
            };

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(a => a.Id == userId);
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;
            shoppingCartVM.OrderHeader.ApplicationUser = applicationUser;
            shoppingCartVM.OrderHeader.Name = applicationUser.FullName;
            shoppingCartVM.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            shoppingCartVM.OrderHeader.StreetAddress = applicationUser.StreetAddress;
            shoppingCartVM.OrderHeader.City = applicationUser.City;
            shoppingCartVM.OrderHeader.State = applicationUser.State;
            shoppingCartVM.OrderHeader.PostalCode = applicationUser.PostalCode;

            foreach(var cart in shoppingCartVM.Carts)
            {
                shoppingCartVM.OrderHeader.TotalOrder += GetPriceBaseOnQuantity(cart) * cart.Quantity;
            }

            return View(shoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(a => a.Id == userId);

            ShoppingCartVM.Carts = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == userId, "Product");
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;

            foreach (var cart in ShoppingCartVM.Carts)
            {
                cart.Price = GetPriceBaseOnQuantity(cart);
                ShoppingCartVM.OrderHeader.TotalOrder += cart.Price * cart.Quantity;
            }

            if(applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            }
            else
            {
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            }

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach(var cart in ShoppingCartVM.Carts)
            {
               OrderDetailModel orderDetail = new()
                {
                    Quantity = cart.Quantity,
                    Price = cart.Price,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    ProductId = cart.ProductId,
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
            }
            _unitOfWork.Save();

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // TODO - STRIPE PAYMENT GATE WAY
                string domain = "https://localhost:7106";
                var options = new SessionCreateOptions
                {
                    CancelUrl = $"{domain}/customer/cart/index",
                    SuccessUrl = $"{domain}/customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach(var shopingCart in ShoppingCartVM.Carts)
                {
                    var sessionLineItem = new SessionLineItemOptions()
                    {
                        PriceData = new SessionLineItemPriceDataOptions()
                        {
                            UnitAmount = (long)(shopingCart.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions()
                            {
                                Name = shopingCart.Product.Title,
                            }
                        },
                        Quantity = shopingCart.Quantity,
                    };

                    options.LineItems.Add(sessionLineItem);
                }
                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);

                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderConfirmation), new {id=ShoppingCartVM.OrderHeader.Id});
        }

        public IActionResult OrderConfirmation(int id)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(x => x.Id == id, includeProperties: "ApplicationUser");
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var sessionService = new SessionService();
                Session session = sessionService.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {

                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(id, orderHeader.SessionId, session.PaymentIntentId);
                    _unitOfWork.Save();
                }
            }

            List<ShoppingCartModel> shoppingCartItems = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.DeleteMany(shoppingCartItems);
            _unitOfWork.Save();
            HttpContext.Session.Clear();

            return View(id);
        }

        private double GetPriceBaseOnQuantity(ShoppingCartModel cart)
        {
            if(cart.Quantity <= 50)
            {
                return cart.Product.Price;
            }
            else
            {
                if(cart.Quantity <= 100)
                {
                    return cart.Product.Price50;
                }
                return cart.Product.Price100;   
            }
        }
    }
}
