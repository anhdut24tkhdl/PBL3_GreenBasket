using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PPL3_Banhangonline.Database;

namespace PPL3_Banhangonline.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContext _context;

        public ShopController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Detail(int id)
        {
            var shop = _context.Shops
                .Include(s => s.Seller)
                .Include(s => s.Products)
                    .ThenInclude(p => p.Category)
                .FirstOrDefault(s => s.ShopID == id);

            if (shop == null)
                return Content("Không tìm thấy cửa hàng.");

            return View(shop);
        }
    }
}