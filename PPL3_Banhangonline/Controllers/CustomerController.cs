using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PPL3_Banhangonline.Database;
using PPL3_Banhangonline.Models.Viewmodels;
using PPL3_Banhangonline.Models;

namespace PPL3_Banhangonline.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        private IActionResult? KiemTraCustomer()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            var role = HttpContext.Session.GetString("Role");

            if (accountId == null || role?.ToLower() != "customer")
            {
                return RedirectToAction("Login", "Account");
            }

            return null;
        }

        public IActionResult Orders(string? status, DateTime? fromDate, DateTime? toDate, decimal? minAmount, decimal? maxAmount, string? keyword)
        {
            if (KiemTraCustomer() != null)
            {
                return KiemTraCustomer()!;
            }

            var accountId = HttpContext.Session.GetInt32("AccountId")!.Value;
            var customer = _context.Customers.FirstOrDefault(c => c.UserID == accountId);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Home");
            }

            var regularOrdersQuery = _context.Orders
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.CustomerID == customer.CustomerID)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                regularOrdersQuery = regularOrdersQuery.Where(o => o.Status == status);
            }

            if (fromDate.HasValue)
            {
                var from = fromDate.Value.Date;
                regularOrdersQuery = regularOrdersQuery.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date >= from);
            }

            if (toDate.HasValue)
            {
                var to = toDate.Value.Date;
                regularOrdersQuery = regularOrdersQuery.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date <= to);
            }

            if (minAmount.HasValue)
            {
                regularOrdersQuery = regularOrdersQuery.Where(o => o.TotalAmount.HasValue && o.TotalAmount.Value >= minAmount.Value);
            }

            if (maxAmount.HasValue)
            {
                regularOrdersQuery = regularOrdersQuery.Where(o => o.TotalAmount.HasValue && o.TotalAmount.Value <= maxAmount.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var searchKeyword = keyword.Trim();
                var isOrderId = int.TryParse(searchKeyword, out var orderId);

                regularOrdersQuery = regularOrdersQuery.Where(o =>
                    (isOrderId && o.OrderID == orderId) ||
                    o.OrderDetails.Any(od => od.Product != null &&
                                             od.Product.ProductName != null &&
                                             od.Product.ProductName.Contains(searchKeyword)));
            }

            var rescueOrders = _context.RescueRegistrations
                .Include(r => r.Campaign)
                .Where(r => r.CustomerID == customer.CustomerID)
                .OrderByDescending(r => r.RegistrationDate)
                .ToList();

            var model = new CustomerOrdersViewModel
            {
                RegularOrders = regularOrdersQuery.ToList(),
                RescueRegistrations = rescueOrders,
                CurrentStatus = status,
                CurrentFromDate = fromDate,
                CurrentToDate = toDate,
                CurrentMinAmount = minAmount,
                CurrentMaxAmount = maxAmount,
                CurrentKeyword = keyword
            };

            return View(model);
        }

        public IActionResult OrderDetail(int id)
        {
            if (KiemTraCustomer() != null)
            {
                return KiemTraCustomer()!;
            }

            var accountId = HttpContext.Session.GetInt32("AccountId")!.Value;
            var customer = _context.Customers.FirstOrDefault(c => c.UserID == accountId);
            if (customer == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.CustomerID != customer.CustomerID)
            {
                return RedirectToAction(nameof(Orders));
            }

            return View(order);
        }

        [HttpGet]
        public IActionResult EditProfile()
        {
            if (KiemTraCustomer() != null)
            {
                return KiemTraCustomer()!;
            }

            var accountId = HttpContext.Session.GetInt32("AccountId")!.Value;
            var customer = _context.Customers.FirstOrDefault(c => c.UserID == accountId);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Home");
            }

            var model = new CustomerProfileViewModel
            {
                Name = customer.Name,
                Phone = customer.Phone,
                Email = customer.Email,
                Address = customer.Address,
                Age = customer.Age
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(CustomerProfileViewModel model)
        {
            if (KiemTraCustomer() != null)
            {
                return KiemTraCustomer()!;
            }

            var accountId = HttpContext.Session.GetInt32("AccountId")!.Value;
            var customer = _context.Customers.FirstOrDefault(c => c.UserID == accountId);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var emailDaTonTai = _context.Customers
                .Any(c => c.CustomerID != customer.CustomerID && c.Email == model.Email && model.Email != null);
            if (emailDaTonTai)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại.");
                return View(model);
            }

            customer.Name = model.Name;
            customer.Phone = model.Phone;
            customer.Email = model.Email;
            customer.Address = model.Address;
            customer.Age = model.Age;
            _context.SaveChanges();

            TempData["Success"] = "Cập nhật thông tin thành công.";
            return RedirectToAction(nameof(EditProfile));
        }
        [HttpGet]
        public IActionResult ReviewProduct(int orderId, int productId)
        {
            if (KiemTraCustomer() != null)
                return KiemTraCustomer()!;

            var accountId = HttpContext.Session.GetInt32("AccountId")!.Value;
            var customer = _context.Customers.FirstOrDefault(c => c.UserID == accountId);

            if (customer == null)
                return RedirectToAction("Index", "Home");

            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderID == orderId && o.CustomerID == customer.CustomerID);

            if (order == null)
                return NotFound();

            if (order.Status != "Delivered")
            {
                TempData["Error"] = "Bạn chỉ có thể đánh giá sau khi đơn hàng đã giao thành công.";
                return RedirectToAction("OrderDetail", new { id = orderId });
            }

            var orderDetail = order.OrderDetails.FirstOrDefault(od => od.ProductID == productId);
            if (orderDetail == null)
                return NotFound();

            var existedReview = _context.Reviews.Any(r =>
                r.CustomerID == customer.CustomerID &&
                r.ProductID == productId &&
                r.OrderID == orderId);

            if (existedReview)
            {
                TempData["Error"] = "Bạn đã đánh giá sản phẩm này rồi.";
                return RedirectToAction("OrderDetail", new { id = orderId });
            }

            var model = new ReviewViewModel
            {
                OrderID = orderId,
                ProductID = productId,
                ProductName = orderDetail.Product?.ProductName
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReviewProduct(ReviewViewModel model)
        {
            if (KiemTraCustomer() != null)
                return KiemTraCustomer()!;

            var accountId = HttpContext.Session.GetInt32("AccountId")!.Value;
            var customer = _context.Customers.FirstOrDefault(c => c.UserID == accountId);

            if (customer == null)
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View(model);

            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderID == model.OrderID && o.CustomerID == customer.CustomerID);

            if (order == null)
                return NotFound();

            if (order.Status != "Delivered")
            {
                TempData["Error"] = "Bạn chỉ có thể đánh giá sau khi đơn hàng đã giao thành công.";
                return RedirectToAction("OrderDetail", new { id = model.OrderID });
            }

            var hasProduct = order.OrderDetails.Any(od => od.ProductID == model.ProductID);
            if (!hasProduct)
                return NotFound();

            var existedReview = _context.Reviews.Any(r =>
                r.CustomerID == customer.CustomerID &&
                r.ProductID == model.ProductID &&
                r.OrderID == model.OrderID);

            if (existedReview)
            {
                TempData["Error"] = "Bạn đã đánh giá sản phẩm này rồi.";
                return RedirectToAction("OrderDetail", new { id = model.OrderID });
            }

            var review = new Review
            {
                CustomerID = customer.CustomerID,
                ProductID = model.ProductID,
                OrderID = model.OrderID,
                Rating = model.Rating,
                Comment = model.Comment,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            _context.SaveChanges();

            TempData["Success"] = "Đánh giá sản phẩm thành công.";
            return RedirectToAction("OrderDetail", new { id = model.OrderID });
        }
    }
}
