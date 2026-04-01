using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PPL3_Banhangonline.Database;
using PPL3_Banhangonline.Models;
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
    }
}
