using System.Collections.Generic;

namespace FinanceAPICore.DataService
{
    public interface INotificationDataService
    {
        List<Notification> GetNotifications(string clientId);
        Notification GetNotificationById(string id, string clientId);
        bool InsertNotification(Notification notification);
        bool UpdateNotification(Notification notification);
        bool DeleteNotification(string id, string clientId);
    }
}