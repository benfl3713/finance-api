using System;
using System.Collections.Generic;
using FinanceAPICore.DataService;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FinanceAPICore
{
    public class Notification
    {
        public Notification()
        {
        }

        public Notification(string clientId, string message, NotificationTypes notificationType = NotificationTypes.Info)
        {
            ClientID = clientId;
            Message = message;
            NotificationType = notificationType;
            ID = Guid.NewGuid().ToString();
            DateCreated = DateTime.Now;
        }
        
        [BsonId]
        public string ID { get; set; }
        [JsonIgnore] 
        public string ClientID { get; set; }
        public string Message { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public NotificationTypes NotificationType { get; set; }
        public Dictionary<string, string> Details { get; set; }
        public bool MarkedAsRead { get; set; } = false;
        public DateTime DateCreated { get; set; }

        public enum NotificationTypes
        {
            Info,
            Warning,
            Error
        }

        public static void InsertNew(Notification notification)
        {
            NotificationDataService?.InsertNotification(notification);
        }

        public static INotificationDataService NotificationDataService;

    }
}