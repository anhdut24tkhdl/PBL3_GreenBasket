using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PPL3_Banhangonline.Database;
using PPL3_Banhangonline.Models;
using PPL3_Banhangonline.Models.Viewmodels;

namespace PPL3_Banhangonline.Controllers
{
    public class SellerController : Controller
    {
        private readonly AppDbContext _context;

        public SellerController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            var role = HttpContext.Session.GetString("Role");

            if (accountId == null || role?.ToLower() != "seller")
            {
                return RedirectToAction("Login", "Account");
            }

            var seller = _context.Sellers
                .Include(s => s.Shop)
                .FirstOrDefault(s => s.UserID == accountId);

            if (seller == null)
            {
                return Content("Không tìm thấy seller.");
            }

            if (seller.Shop == null)
            {
                return Content("Seller này chưa có cửa hàng.");
            }

            return View(seller.Shop);
        }

        public IActionResult ManageProducts()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            var role = HttpContext.Session.GetString("Role");

            if (accountId == null || role?.ToLower() != "seller")
            {
                return RedirectToAction("Login", "Account");
            }

            var seller = _context.Sellers
                .Include(s => s.Shop)
                .FirstOrDefault(s => s.UserID == accountId);

            if (seller == null || seller.Shop == null)
            {
                return Content("Không tìm thấy cửa hàng của seller.");
            }

            var products = _context.Products
                .Where(p => p.ShopID == seller.Shop.ShopID)
                .ToList();

            ViewBag.ShopName = seller.Shop.ShopName;

            return View(products);
        }

        [HttpGet]
        public IActionResult RegisterSeller()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            var role = HttpContext.Session.GetString("Role");

            if (accountId == null || role?.ToLower() != "customer")
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public IActionResult RegisterSeller(RegisterSellerViewModel model)
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            var role = HttpContext.Session.GetString("Role");

            if (accountId == null || role?.ToLower() != "customer")
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            // Kiểm tra mật khẩu đúng không
            var account = _context.Account.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null || account.Password != model.Password)
            {
                ViewBag.Error = "Mật khẩu không đúng.";
                return View(model);
            }

            // Kiểm tra đã là seller chưa
            var existingSeller = _context.Sellers.FirstOrDefault(s => s.UserID == accountId);
            if (existingSeller != null)
            {
                ViewBag.Error = "Tài khoản này đã đăng ký seller rồi.";
                return View(model);
            }

            // Lấy thông tin từ Customer đã có
            var customer = _context.Customers.FirstOrDefault(c => c.UserID == accountId);
            if (customer == null)
            {
                ViewBag.Error = "Không tìm thấy thông tin khách hàng.";
                return View(model);
            }

            // Tạo Seller từ thông tin Customer
            var seller = new Seller
            {
                UserID = accountId.Value,
                Name = customer.Name,
                Phone = customer.Phone,
                Email = customer.Email,
                Address = customer.Address,
                Age = customer.Age
            };

            _context.Sellers.Add(seller);
            _context.SaveChanges();

            // Tạo Shop
            var shop = new Shop
            {
                SellerID = seller.SellerID,
                ShopName = model.ShopName
            };
            _context.Shops.Add(shop);

            // Đổi Role → seller
            account.Role = "seller";
            _context.SaveChanges();

            // Cập nhật session
            HttpContext.Session.SetString("Role", "seller");

            TempData["Success"] = "Đăng ký seller thành công!";
            return RedirectToAction("Index", "Seller");
        }

        public IActionResult ManageOrders(string status = "Processing")
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            var seller = _context.Sellers.Include(s => s.Shop).FirstOrDefault(s => s.UserID == accountId);

            if (seller?.Shop == null) return RedirectToAction("Index");

            // Lấy các đơn hàng có chứa sản phẩm thuộc Shop của Seller này
            var orders = _context.OrderDetails
                .Include(od => od.Order)
                .ThenInclude(o => o.Customer)
                .Include(od => od.Product)
                .Where(od => od.Product.ShopID == seller.Shop.ShopID && od.Order.Status == status)
                .Select(od => od.Order)
                .Distinct()
                .ToList();

            ViewBag.CurrentStatus = status;
            ViewBag.ShopName = seller.Shop.ShopName;
            return View(orders);
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string newStatus)
        {
            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = newStatus;
                _context.SaveChanges();
                TempData["Success"] = $"Đã cập nhật đơn hàng #{orderId} thành: {newStatus}";
            }
            return RedirectToAction("ManageOrders", new { status = newStatus });
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            var seller = _context.Sellers.Include(s => s.Shop).FirstOrDefault(s => s.UserID == accountId);

            if (ModelState.IsValid)
            {
                product.ShopID = seller.Shop.ShopID; // Gán sản phẩm vào đúng Shop của Seller này
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("ManageProducts");
            }
            return View(product);
        }
        [HttpGet]
        public IActionResult CreateProduct()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null) return RedirectToAction("Login", "Account");

            // Lấy danh sách loại để chọn
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }
        // ================= CHỈNH SỬA SẢN PHẨM =================
        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                // Giữ lại ShopID cũ (tránh bị null khi update)
                var existingProduct = _context.Products.AsNoTracking().FirstOrDefault(p => p.ProductID == product.ProductID);
                product.ShopID = existingProduct.ShopID;

                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction("ManageProducts");
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            try
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
                TempData["Success"] = "Đã xóa sản phẩm thành công.";
            }
            catch (Exception)
            {
                // Nếu lỗi do ràng buộc khóa ngoại (đã có người mua hàng này)
                TempData["Error"] = "Không thể xóa sản phẩm này vì đã có trong đơn hàng hoặc giỏ hàng của khách!";
            }

            return RedirectToAction("ManageProducts");
        }
        
    }
}
