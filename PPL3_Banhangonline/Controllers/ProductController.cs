using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PPL3_Banhangonline.Database;
using PPL3_Banhangonline.Models;

namespace PPL3_Banhangonline.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        private Shop GetCurrentSellerShop()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            var role = HttpContext.Session.GetString("Role");

            if (accountId == null || role?.ToLower() != "seller")
            {
                return null;
            }

            var seller = _context.Sellers
                .Include(s => s.Shop)
                .FirstOrDefault(s => s.UserID == accountId);

            if (seller == null || seller.Shop == null)
            {
                return null;
            }

            return seller.Shop;
        }

        private void LoadCategories(object selectedCategory = null)
        {
            ViewBag.Categories = new SelectList(
                _context.Categories.ToList(),
                "CategoryID",
                "CategoryName",
                selectedCategory
            );
        }

        public IActionResult Index()
        {
            var shop = GetCurrentSellerShop();
            if (shop == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var products = _context.Products
                .Where(p => p.ShopID == shop.ShopID)
                .ToList();

            ViewBag.ShopName = shop.ShopName;
            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var shop = GetCurrentSellerShop();
            if (shop == null)
            {
                return RedirectToAction("Login", "Account");
            }

            LoadCategories();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product model)
        {
            var shop = GetCurrentSellerShop();
            if (shop == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                LoadCategories(model.CategoryID);
                return View(model);
            }

            model.ShopID = shop.ShopID;

            _context.Products.Add(model);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var shop = GetCurrentSellerShop();
            if (shop == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _context.Products
                .FirstOrDefault(p => p.ProductID == id && p.ShopID == shop.ShopID);

            if (product == null)
            {
                return Content("Không tìm thấy sản phẩm.");
            }

            LoadCategories(product.CategoryID);
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product model)
        {
            var shop = GetCurrentSellerShop();
            if (shop == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _context.Products
                .FirstOrDefault(p => p.ProductID == model.ProductID && p.ShopID == shop.ShopID);

            if (product == null)
            {
                return Content("Không tìm thấy sản phẩm.");
            }

            if (!ModelState.IsValid)
            {
                LoadCategories(model.CategoryID);
                return View(model);
            }

            product.ProductName = model.ProductName;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Stock = model.Stock;
            product.Image = model.Image;
            product.CategoryID = model.CategoryID;
            

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var shop = GetCurrentSellerShop();
            if (shop == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductID == id && p.ShopID == shop.ShopID);

            if (product == null)
            {
                return Content("Không tìm thấy sản phẩm.");
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var shop = GetCurrentSellerShop();
            if (shop == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _context.Products
                .FirstOrDefault(p => p.ProductID == id && p.ShopID == shop.ShopID);

            if (product == null)
            {
                return Content("Không tìm thấy sản phẩm.");
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult ByCategory(int id)
        {
            var category = _context.Categories.FirstOrDefault(c => c.CategoryID == id);

            var products = _context.Products
                .Include(p => p.Shop)
                .ThenInclude(s => s.Seller)
                .Where(p => p.CategoryID == id)
                .ToList();

            ViewBag.CategoryName = category?.CategoryName;

            return View(products);  
        }

        public IActionResult Search(string keyword, int? categoryId)
        {
            var query = _context.Products
                .Include(p => p.Shop)
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(p => p.ProductName.Contains(keyword));

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryID == categoryId.Value);

            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = _context.Categories.ToList();

            return View(query.ToList());
        }
        public IActionResult Detail(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Shop)
                    .ThenInclude(s => s.Seller)
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
                return Content("Không tìm thấy sản phẩm.");

            return View(product);
        }
    }
}