using System.Collections.Generic;
using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.DataService.Wealth;
using FinanceAPICore.Wealth;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FinanceAPIMongoDataService.DataService.Wealth
{
    public class TradeDataService : BaseDataService, ITradeDataService
    {
        private readonly string _connectionString;
        private static string databaseName = "finance";
        private static string tableName = "trades";

        public TradeDataService(IOptions<AppSettings> appSettings) : base(appSettings)
        {
            _connectionString = appSettings.Value.MongoDB_ConnectionString;
        }

        public bool Insert(Trade trade)
        {
            MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
            return database.InsertRecord(tableName, trade);
        }

        public bool Update(Trade trade)
        {
            MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
            var filter = Builders<Trade>.Filter.Eq(nameof(Trade.ClientID), trade.ClientID);
            return database.UpdateRecord(tableName, trade, trade.Id, filter, nameof(Trade.Id));
        }

        public bool Delete(string id, string clientId)
        {
            MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
            var filter = Builders<Trade>.Filter.Eq(nameof(Trade.ClientID), clientId);
            return database.DeleteRecord(tableName, id, filter, nameof(Trade.Id));
        }

        public List<Trade> GetTrades(string clientId)
        {
            MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
            FilterDefinition<Trade> filter = Builders<Trade>.Filter.Eq(nameof(Trade.ClientID), clientId);
            return database.LoadRecordsByFilter(tableName, filter);
        }

        public Trade GetTradeById(string id, string clientId)
        {
            MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
            Trade trade = database.LoadRecordById<Trade>(tableName, id, nameof(Trade.Id));

            if (trade == null || trade.ClientID != clientId)
                throw new KeyNotFoundException($"Could not find Trade {id}");

            return trade;
        }
    }
}