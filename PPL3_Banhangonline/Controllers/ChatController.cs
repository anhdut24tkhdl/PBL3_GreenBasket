using Microsoft.AspNetCore.Mvc;
using PPL3_Banhangonline.Models;
using PPL3_Banhangonline.Service;

namespace PPL3_Banhangonline.Controllers
{
    public class ChatController : Controller
    {
        private readonly AIService _aiService;

        public ChatController(AIService aiService)
        {
            _aiService = aiService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new ChatViewModel();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Ask(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return Json(new { reply = "Bạn chưa nhập gì." });

            var reply = await _aiService.AskGeminiAsync(message);

            return Json(new { reply });
        }
        [HttpPost]
        public async Task<IActionResult> Index(ChatViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.UserMessage))
            {
                return View(model);
            }

            model.Messages.Add(new ChatMessage
            {
                Role = "user",
                Content = model.UserMessage
            });

            var reply = await _aiService.AskGeminiAsync(model.UserMessage);

            model.Messages.Add(new ChatMessage
            {
                Role = "assistant",
                Content = reply
            });

            model.UserMessage = "";
            return View(model);
        }
    }
}