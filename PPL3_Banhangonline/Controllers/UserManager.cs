using Microsoft.AspNetCore.Mvc;
using PPL3_Banhangonline.Database;

namespace PPL3_Banhangonline.Controllers
{
    public class UserManager : Controller
    {

        private readonly AppDbContext _context;

        public UserManager(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var users = _context.Account.ToList();
            return View(users);
        }
    }
}
