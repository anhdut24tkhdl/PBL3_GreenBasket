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

            // --- BƯỚC CHECK KHO QUAN TRỌNG ---
            int currentQtyInCart = cartItem?.Quantity ?? 0;
            if (product.Stock < currentQtyInCart + 1)
            {
                TempData["Message"] = $"Sản phẩm {product.ProductName} chỉ còn {product.Stock} món, không đủ để thêm tiếp!";
                return RedirectToAction("Index", "Home");
            }

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

        public IActionResult UpdateQuantity(int cartItemId, int quantity)
        {
            var customerId = GetCustomerId();
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product) 
                .FirstOrDefault(ci => ci.CartItemID == cartItemId && ci.Cart.CustomerID == customerId);

            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
                else
                {   
                    // KIỂM TRA KHO Ở ĐÂY
                    if (quantity > cartItem.Product.Stock)
                    {
                        TempData["Message"] = $"Sản phẩm {cartItem.Product.ProductName} chỉ còn {cartItem.Product.Stock} sản phẩm trong kho.";
                        return RedirectToAction("Index");
                    }
                    cartItem.Quantity = quantity;
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Checkout(List<int> selectedIds)
        {
            var customerId = GetCustomerId();
            if (customerId == null) return RedirectToAction("Login", "Account");

            // Kiểm tra xem khách đã tích chọn món nào chưa
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["Message"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán.";
                return RedirectToAction("Index");
            }

            // Lấy giỏ hàng và CHỈ lọc ra các món có ID nằm trong danh sách đã chọn
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.CustomerID == customerId);

            if (cart != null)
            {
                // Lọc lại danh sách món hàng theo đúng những gì khách đã tích ở Index
                cart.CartItems = cart.CartItems
                    .Where(ci => selectedIds.Contains(ci.CartItemID))
                    .ToList();
            }

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Message"] = "Không tìm thấy sản phẩm hợp lệ.";
                return RedirectToAction("Index");
            }

            // Trả về view Checkout với giỏ hàng đã được lọc
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(List<int> selectedCartItemIds)
        {
            var customerId = GetCustomerId();
            if (customerId == null) return RedirectToAction("Login", "Account");

            if (selectedCartItemIds == null || !selectedCartItemIds.Any()) return RedirectToAction("Index");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.CustomerID == customerId);

            // Lọc ra các món thực sự nằm trong danh sách thanh toán
            var itemsToPurchase = cart.CartItems
                .Where(ci => selectedCartItemIds.Contains(ci.CartItemID))
                .ToList();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo đơn hàng (Order)
                var order = new Order
                {
                    CustomerID = customerId.Value,
                    OrderDate = DateTime.Now,
                    Status = "Processing",
                    TotalAmount = itemsToPurchase.Sum(ci => ci.Quantity * ci.Price)
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 2. Tạo chi tiết (OrderDetail) và Trừ kho
                foreach (var item in itemsToPurchase)
                {
                    _context.OrderDetails.Add(new OrderDetail
                    {
                        OrderID = order.OrderID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    });

                    var product = await _context.Products.FindAsync(item.ProductID);
                    if (product != null)
                    {
                        if (product.Stock < item.Quantity)
                            throw new Exception($"Sản phẩm {product.ProductName} không đủ hàng.");
                        product.Stock -= item.Quantity;
                    }
                }

                // 3. Tạo Payment
                _context.Payments.Add(new Payment
                {
                    OrderID = order.OrderID,
                    Method = "COD",
                    Amount = order.TotalAmount,
                    PaymentDate = DateTime.Now,
                    Status = "Pending"
                });

                // 4. QUAN TRỌNG: Chỉ xóa những món ĐÃ MUA khỏi giỏ hàng
                _context.CartItems.RemoveRange(itemsToPurchase);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("OrderSuccess", new { id = order.OrderID });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Message"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public IActionResult RemoveSelected(List<int> selectedIds)
        {
            var customerId = GetCustomerId();
            if (customerId == null) return RedirectToAction("Login", "Account");

            if (selectedIds != null && selectedIds.Any())
            {
                // Tìm các món hàng có ID nằm trong danh sách chọn và thuộc về đúng khách hàng này
                var itemsToRemove = _context.CartItems
                    .Include(ci => ci.Cart)
                    .Where(ci => selectedIds.Contains(ci.CartItemID) && ci.Cart.CustomerID == customerId)
                    .ToList();

                if (itemsToRemove.Any())
                {
                    _context.CartItems.RemoveRange(itemsToRemove);
                    _context.SaveChanges();
                    TempData["Message"] = $"đã xóa {itemsToRemove.Count} sản phẩm khỏi giỏ hàng.";
                }
            }
            else
            {
                TempData["Message"] = "Vui lòng chọn sản phẩm cần xóa.";
            }

            return RedirectToAction("Index");
        }
        // Hàm hiển thị trang thông báo thành công
        public IActionResult OrderSuccess(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

    }
}