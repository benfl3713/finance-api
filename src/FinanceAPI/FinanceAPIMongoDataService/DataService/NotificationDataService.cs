using System.Collections.Generic;
using System.Linq;
using FinanceAPICore;
using FinanceAPICore.DataService;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FinanceAPIMongoDataService.DataService
{
    public class NotificationDataService : BaseDataService, INotificationDataService
    {
        private static string databaseName = "finance";
        private static string tableName = "notifications";
        private readonly string _connectionString;
        public NotificationDataService(IOptions<AppSettings> appSettings) : base(appSettings)
        {
            _connectionString = appSettings.Value.MongoDB_ConnectionString;
        }

        public List<Notification> GetNotifications(string clientId)
        {
            MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
            FilterDefinition<Notification> filter = Builders<Notification>.Filter.Eq("ClientID", clientId);
            return database.LoadRecordsByFilter(tableName, filter).OrderByDescending(t => t.DateCreated).ToList();
        }

        public Notification GetNotificationById(string id, string clientId)
        {
            MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
            Notification notification = database.LoadRecordById<Notification>(tableName, id);

            if (notification == null)
                return null;
            
            return notification.ClientID == clientId ? notification : null;
        }

        public bool InsertNotification(Notification notification)
        {
            MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
            return database.InsertRecord(tableName, notification);
        }

        public bool UpdateNotification(Notification notification)
        {
            MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
            return database.UpdateRecord(tableName, notification, notification.ID);
        }

        public bool DeleteNotification(string id, string clientId)
        {
            MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
            FilterDefinition<Notification> filter = Builders<Notification>.Filter.Eq(nameof(Notification.ClientID), clientId);
            return database.DeleteRecord(tableName, id, filter);
        }
    }
}