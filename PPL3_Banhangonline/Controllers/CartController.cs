using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PPL3_Banhangonline.Database;
using PPL3_Banhangonline.Models;
using PPL3_Banhangonline.ViewModels;

namespace PPL3_Banhangonline.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private int? GetCustomerId()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
                return null;

            var customer = _context.Customers
                .FirstOrDefault(c => c.UserID == accountId.Value);

            if (customer == null)
                return null;

            return customer.CustomerID;
        }

        private Cart GetOrCreateCart(int customerId)
        {
            var cart = _context.Carts.FirstOrDefault(c => c.CustomerID == customerId);

            if (cart == null)
            {
                cart = new Cart
                {
                    CustomerID = customerId,
                    CreatedAt = DateTime.Now
                };

                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            return cart;
        }

        public IActionResult Index()
        {
            var customerId = GetCustomerId();
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.CustomerID == customerId);

            var vm = new CartViewModel();

            if (cart != null && cart.CartItems != null)
            {
                vm.Items = cart.CartItems.Select(ci => new CartItemViewModel
                {
                    CartItemID = ci.CartItemID,
                    ProductID = ci.ProductID,
                    ProductName = ci.Product.ProductName,
                    Image = ci.Product.Image,
                    UnitPrice = ci.Price,
                    Quantity = ci.Quantity
                }).ToList();
            }

            return View(vm);
        }

        public IActionResult AddToCart(int productId)
        {
            var customerId = GetCustomerId();
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _context.Products.FirstOrDefault(p => p.ProductID == productId);
            if (product == null)
            {
                return NotFound();
            }

            var cart = GetOrCreateCart(customerId.Value);

            var cartItem = _context.CartItems
                .FirstOrDefault(ci => ci.CartID == cart.CartID && ci.ProductID == productId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartID = cart.CartID,
                    ProductID = product.ProductID,
                    Quantity = 1,
                    Price = (decimal)product.Price
                };

                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += 1;
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Remove(int cartItemId)
        {
            var customerId = GetCustomerId();
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefault(ci => ci.CartItemID == cartItemId && ci.Cart.CustomerID == customerId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            var customerId = GetCustomerId();
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefault(ci => ci.CartItemID == cartItemId && ci.Cart.CustomerID == customerId);

            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var customerId = GetCustomerId();
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.CustomerID == customerId);

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                TempData["Message"] = "Giỏ hàng không có sản phẩm.";
                return RedirectToAction("Index");
            }

            return View(cart);
        }
    }
}