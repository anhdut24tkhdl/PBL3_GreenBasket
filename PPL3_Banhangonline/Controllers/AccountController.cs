using Microsoft.AspNetCore.Mvc;
using PPL3_Banhangonline.Database;
using PPL3_Banhangonline.Models;

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
            HttpContext.Session.SetInt32("UserId", user.AccountId);
            HttpContext.Session.SetString("Username", user.AccountName);
            HttpContext.Session.SetString("Role", user.Role);

            return RedirectToAction("Index", "Home");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
