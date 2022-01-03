using System;
using System.Collections.Generic;
using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.DataService.Wealth;
using FinanceAPICore.Wealth;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FinanceAPIMongoDataService.DataService.Wealth
{
    public class AssetDataService : BaseDataService, IAssetDataService
    {
        private readonly string _connectionString;
        private static string databaseName = "finance";
        private static string tableName = "assets";
        private static string priceTableName = "assetprices";
        
        public AssetDataService(IOptions<AppSettings> appSettings) : base(appSettings)
        {
            _connectionString = appSettings.Value.MongoDB_ConnectionString;
        }

        public bool InsertAsset(Asset asset)
        {
            MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
            return database.InsertRecord(tableName, asset);
        }

        public bool UpdateAsset(Asset asset)
        {
            MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
            var filter = Builders<Asset>.Filter.Eq(nameof(Asset.ClientId), asset.ClientId);
            bool result =  database.UpdateRecord(tableName, asset, asset.Id, filter, nameof(Asset.Id));

            // Create new price history record
            database.InsertRecord(priceTableName, new AssetPrice(asset, asset.LastUpdated));

            return result;
        }

        public bool DeleteAsset(string id, string clientId)
        {
            MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
            var filter = Builders<Asset>.Filter.Eq(nameof(Asset.ClientId), clientId);
            return database.DeleteRecord(tableName, id, filter, nameof(Asset.Id));
        }

        public List<Asset> GetAssets(string clientId)
        {
            MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
            FilterDefinition<Asset> filter = Builders<Asset>.Filter.Eq(nameof(Asset.ClientId), clientId);
            return database.LoadRecordsByFilter(tableName, filter);
        }

        public Asset GetAssetById(string assetId, string clientId)
        {
            MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
            Asset asset = database.LoadRecordById<Asset>(tableName, assetId, nameof(Asset.Id));

            if (asset == null || asset.ClientId != clientId)
                throw new KeyNotFoundException($"Could not find Asset {assetId}");

            return asset;
        }
    }
}