using FinanceAPICore;
using FinanceAPICore.DataService;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIMongoDataService.DataService
{
	public class DatafeedDataService : IDatafeedDataService
	{
		static string databaseName = "finance";
		static string datafeedTableName = "datafeeds";
		static string externalAccountMappingsTable = "datafeed_account_mappings";

		public bool AddAccountDatafeedMapping(string clientId, string datafeed, string vendorID, string accountID, string externalAccountID)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			var record = new
			{
				_id = accountID,
				externalId = externalAccountID,
				datafeed,
				vendorID,
				clientId
			};
			return database.InsertRecord(externalAccountMappingsTable, record);
		}

		public bool RemoveAccountDatafeedMapping(string clientId, string accountID)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			var filter = Builders<Datafeed>.Filter.Eq("clientId", clientId);

			return database.DeleteRecord(externalAccountMappingsTable, accountID, filter, "_id");
		}

		public bool AddUpdateClientDatafeed(Datafeed datafeed)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			return database.UpsertRecord(datafeedTableName, datafeed, datafeed._id);
		}

		public List<Datafeed> GetDatafeeds(string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			var filter = Builders<Datafeed>.Filter.Eq("ClientId", clientId);
			return database.LoadRecordsByFilter(datafeedTableName, filter);
		}

		public string GetExternalAccountMapping(string accountId)
		{
			throw new NotImplementedException();
		}

		public string GetRefreshTokenByAccessKey(string encryptedAccesskey)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			var filter = Builders<Datafeed>.Filter.Eq("AccessKey", encryptedAccesskey);
			var datafeeds = database.LoadRecordsByFilter(datafeedTableName, filter);
			if (datafeeds != null && datafeeds.Count > 0)
				return datafeeds[0].RefreshKey;

			return null;
		}

		public bool IsExternalAccountMapped(string clientId, string externalAccountID, string vendorID, out string mappedAccount)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			var accountFilter = Builders<dynamic>.Filter.Eq("externalId", externalAccountID);
			var vendorFilter = Builders<dynamic>.Filter.Eq("vendorID", vendorID);
			var clientFilter = Builders<dynamic>.Filter.Eq("clientId", clientId);
			
			var accounts = database.LoadRecordsByFilter(externalAccountMappingsTable, accountFilter & vendorFilter & clientFilter);
			if(accounts != null && accounts.Count > 0)
			{
				mappedAccount = accounts[0]._id;
				return true;
			}

			mappedAccount = null;
			return false;
		}

		public bool UpdateAccessKey(string newAccessKey, string newRefreshToken, string oldAccessKey, DateTime lastUpdated)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			var update = Builders<Datafeed>.Update.Set(d => d.AccessKey, newAccessKey);
			update = update.Set(d => d.RefreshKey, newRefreshToken);

			var filter = Builders<Datafeed>.Filter.Eq(d => d.AccessKey, oldAccessKey);

			return database.PartialUpdateRecord(datafeedTableName, update, filter);
		}
	}
}
