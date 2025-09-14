using Estore.DataAccess.Repository.IRepository;
using Estore.Models;
using Estore.Models.ViewModels;
using Estore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace EstoreWeb.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize()]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(x => x.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetailList = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == orderId, includeProperties: "Product")
            };
            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin+","+SD.RoleEmployee)]
        public IActionResult UpdateOrderDetails()
        {
            var orderHeader = OrderVM.OrderHeader;
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(x => x.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = orderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderHeader.PhoneNumber;
            orderHeaderFromDb.City = orderHeader.City;
            orderHeaderFromDb.State = orderHeader.State;
            orderHeaderFromDb.StreetAddress = orderHeader.StreetAddress;
            orderHeaderFromDb.PostalCode = orderHeader.PostalCode;

            if (string.IsNullOrEmpty(orderHeader.Carrier) == false)
            {
                orderHeaderFromDb.Carrier = orderHeader.Carrier;
            }

            if(string.IsNullOrEmpty(orderHeader.TrackingNumber) == false)
            {
                orderHeaderFromDb.TrackingNumber = orderHeader.TrackingNumber;
            }

            if(string.IsNullOrEmpty(orderHeader.PaymentStatus) == false)
            {
                orderHeaderFromDb.OrderStatus = SD.StatusPending;
                orderHeaderFromDb.PaymentStatus = orderHeader.PaymentStatus;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order details updated successfully!";
            return RedirectToAction(nameof(Details), new {orderId = orderHeaderFromDb.Id});

        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public IActionResult StartProcessing()
        {
            var orderHeader = OrderVM.OrderHeader;
            _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();

            TempData["Success"] = "Order details updated successfully!";
            return RedirectToAction(nameof(Details), new { orderId = orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = OrderVM.OrderHeader;
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(x => x.Id == orderHeader.Id, includeProperties: "ApplicationUser");
            orderHeaderFromDb.OrderStatus = SD.StatusShipped;
            orderHeaderFromDb.Carrier = orderHeader.Carrier;
            orderHeaderFromDb.TrackingNumber = orderHeader.TrackingNumber;
            orderHeaderFromDb.ShippingDate = DateTime.Now;

            if(orderHeaderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeaderFromDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Order Shipped successfully!";
            return RedirectToAction(nameof(Details), new { orderId = orderHeader.Id });

        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin+"," + SD.RoleEmployee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = OrderVM.OrderHeader;
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(x => x.Id == orderHeader.Id);
            
            if(orderHeaderFromDb.PaymentStatus == SD.PaymentStatusApproved)
            {
                var option = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDb.PaymentIntentId
                };

                var refundService = new RefundService();
                Refund refund = refundService.Create(option);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.PaymentStatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.PaymentStatusCancelled);
            }
            _unitOfWork.Save();
            TempData["Success"] = "Order Cancelled successfully!";
            return RedirectToAction(nameof(Details), new {orderId =  orderHeader.Id});
        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult PayNow()
        {
            var orderHeader = OrderVM.OrderHeader;
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(x => x.Id == orderHeader.Id);
            var orderDetails = _unitOfWork.OrderDetail.GetAll(x => x.Id == orderHeader.Id, includeProperties: "Product");    
            
            string domain = "https://localhost:7106";
            var options = new SessionCreateOptions()
            {
                CancelUrl = $"{domain}/admin/Order/Details?orderId={orderHeader.Id}",
                SuccessUrl = $"{domain}/admin/Order/PaymentConfirmation?orderId={orderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode="payment"
            };

            foreach(var orderDetail in orderDetails)
            {
                var sessionItem = new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        Currency = "usd",
                        UnitAmount = (long) orderDetail.Price * 100,
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = orderDetail.Product.Title
                        }
                    },
                    Quantity = orderDetail.Quantity
                };
                options.LineItems.Add(sessionItem);
            }

            var sessionService = new SessionService();
            Session session = sessionService.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderFromDb.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);

            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderId)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(x => x.Id == orderId);

            if(orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var sessionService = new SessionService();
                Session session = sessionService.Get(orderHeader.SessionId);

                if(session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
                
            }
            return View(orderId);
        }

        [HttpGet]
        public IActionResult GetAll(string? status=null)
        {
            //List<OrderVM> orders = new List<OrderVM>();
            //List<OrderHeaderModel> orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            //foreach (var orderHeader in orderHeaders)
            //{
            //    var orderDtails = _unitOfWork.OrderDetail.GetAll(
            //        x => x.OrderHeaderId == orderHeader.Id,
            //        includeProperties:"OrderHeader,Product"
            //    ).ToList();

            //    var order = new OrderVM
            //    {
            //        OrderHeader = orderHeader,
            //        OrderDetailList = orderDtails
            //    };
            //    orders.Add(order);
            //}

            IEnumerable<OrderHeaderModel> orders;

            if(User.IsInRole(SD.RoleAdmin) || User.IsInRole(SD.RoleEmployee))
            {
                orders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orders = _unitOfWork.OrderHeader.GetAll(x => x.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();
            }

                switch (status)
                {
                    case "pending":
                        orders = orders.Where(x => x.OrderStatus == SD.StatusPending);
                        break;
                    case "approved":
                        orders = orders.Where(x => x.OrderStatus == SD.StatusApproved);
                        break;
                    case "inprocess":
                        orders = orders.Where(x => x.OrderStatus == SD.StatusInProcess);
                        break;
                    case "completed":
                        orders = orders.Where(x => x.OrderStatus == SD.StatusShipped);
                        break;
                    default:
                        break;
                }
            return Json(new { message = "Success", data = orders });
        }
    }
}
