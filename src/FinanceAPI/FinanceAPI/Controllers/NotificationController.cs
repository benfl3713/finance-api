using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FinanceAPI.Attributes;
using FinanceAPICore;
using FinanceAPICore.DataService;
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
        public List<Notification> GetNotifications()
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return _notificationProcessor.GetNotifications(clientId);
        }

        [HttpGet("[action]")]
        public NotificationCountResponse GetUnreadNotificationCount(DateTime? lastChecked = null)
        {
            lastChecked ??= DateTime.Now;
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            List<Notification> notifications = _notificationProcessor.GetNotifications(clientId);
            return new NotificationCountResponse
            {
                Count = notifications.Count(n => !n.MarkedAsRead),
                NewNotifications = notifications.Where(n => n.DateCreated.ToUniversalTime() > lastChecked.Value.ToUniversalTime()).ToList()
            };
        }

        [HttpPut("{notificationId}/[action]")]
        public IActionResult UpdateReadStatus([Required][FromRoute(Name = "notificationId")] string notificationId, [Required] bool isRead)
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return _notificationProcessor.UpdateReadStatus(clientId, notificationId, isRead) ? (IActionResult) Ok() : BadRequest();
        }

        [HttpDelete("{notificationId}")]
        public IActionResult DeleteNotification([Required][FromRoute] string notificationId)
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return _notificationProcessor.DeleteNotification(notificationId, clientId) ? (IActionResult) Ok() : UnprocessableEntity();
        }

        public class NotificationCountResponse
        {
            public int Count { get; set; }
            public List<Notification> NewNotifications { get; set; }
        }
    }
}