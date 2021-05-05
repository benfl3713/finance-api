using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FinanceAPICore
{
    public class Notification
    {
        public Notification()
        {
        }

        public Notification(string message, NotificationTypes notificationType = NotificationTypes.Info)
        {
            Message = message;
            NotificationType = notificationType;
        }
        
        public string Message { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public NotificationTypes NotificationType { get; set; }
        public Dictionary<string, string> details { get; set; }

        public enum NotificationTypes
        {
            Info,
            Warning,
            Error
        }
        
    }
}