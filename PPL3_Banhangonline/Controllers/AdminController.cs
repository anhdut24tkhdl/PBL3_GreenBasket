using Microsoft.AspNetCore.Mvc;

namespace PPL3_Banhangonline.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CategoryList()
        {
            return View();
        }

        public IActionResult UserList()
        {
            return View();
        }
    }
}
