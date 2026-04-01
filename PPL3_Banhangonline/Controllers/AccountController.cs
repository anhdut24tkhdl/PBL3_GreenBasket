using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PPL3_Banhangonline.Database;
using PPL3_Banhangonline.Models;
using PPL3_Banhangonline.Models.Viewmodels;

namespace PPL3_Banhangonline.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Account model)
        {

            var user = _context.Account.FirstOrDefault(x =>
     x.AccountName == model.AccountName && x.Password == model.Password);
            
                
            if (user == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View(model);
            }
           
                
            HttpContext.Session.SetInt32("AccountId", user.AccountId);
            HttpContext.Session.SetString("Username", user.AccountName);
            HttpContext.Session.SetString("Role", user.Role.ToLower());
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var checkUsername = _context.Account.FirstOrDefault(x => x.AccountName == model.Username);
            if (checkUsername != null)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại";
                return View(model);
            }

            var checkEmail = _context.Customers.FirstOrDefault(x => x.Email == model.Email);
            if (checkEmail != null)
            {
                ViewBag.Error = "Email đã tồn tại";
                return View(model);
            }

            var account = new Account
            {
                AccountName = model.Username,
                Password = model.Password,
                Role = "Customer"
            };

            _context.Account.Add(account);
            _context.SaveChanges();

            var customer = new Customer
            {
                UserID = account.AccountId,
                Name = model.Name,
                Phone = model.Phone,
                Email = model.Email,
                Address = model.Address,
                Age = model.Age
            };

            _context.Customers.Add(customer);
            _context.SaveChanges();

            TempData["Success"] = "Đăng ký tài khoản thành công";
            return RedirectToAction("Login", "Account");
        }

    }
}
