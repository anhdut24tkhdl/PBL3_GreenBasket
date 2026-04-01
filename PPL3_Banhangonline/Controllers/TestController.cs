using Microsoft.AspNetCore.Mvc;
using PPL3_Banhangonline.Service;

namespace PPL3_Banhangonline.Controllers
{
    public class TestController : Controller
    {
        private readonly AIService _ai;

        public TestController(AIService ai)
        {
            _ai = ai;
        }

        //public async Task<IActionResult> TestAI()
        //{
        //    var data = await _ai.TestCallAsync();
        //    return Content(data);
        //}
    }
}
