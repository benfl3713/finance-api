using System;
using System.Collections.Generic;
using FinanceAPICore;
using FinanceAPICore.DataService;

namespace FinanceAPIData
{
    public class NotificationProcessor
    {
        private readonly INotificationDataService _notificationDataService;
        public NotificationProcessor(INotificationDataService notificationDataService)
        {
            _notificationDataService = notificationDataService;
        }
        
        public List<Notification> GetNotifications(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId), "clientId is required");

            return _notificationDataService.GetNotifications(clientId);
        }

        public bool UpdateReadStatus(string clientId, string notificationId, bool isRead)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId), "clientId is required");

            if (string.IsNullOrEmpty(notificationId))
                throw new ArgumentNullException(nameof(notificationId), "notificationId is required");

            Notification notification = _notificationDataService.GetNotificationById(notificationId, clientId);
            if (notification == null)
                throw new Exception("Cannot find notification");
            
            notification.MarkedAsRead = isRead;
            return _notificationDataService.UpdateNotification(notification);
        }

        public bool DeleteNotification(string notificationId, string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId), "clientId is required");

            if (string.IsNullOrEmpty(notificationId))
                throw new ArgumentNullException(nameof(notificationId), "notificationId is required");

            return _notificationDataService.DeleteNotification(notificationId, clientId);
        }
    }
}