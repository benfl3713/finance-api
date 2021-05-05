using FinanceAPI.Attributes;
using FinanceAPIData;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAPI.Controllers
{
    [Route("api/notification")]
    [ApiController]
    [Authorize]
    public class NotificationController : Controller
    {
        private NotificationProcessor _notificationProcessor;
        public NotificationController(NotificationProcessor notificationProcessor)
        {
            _notificationProcessor = notificationProcessor;
        }
        
        [HttpGet]
        public IActionResult GetNotifications()
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return Ok();
        }
    }
}