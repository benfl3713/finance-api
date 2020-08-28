using FinanceAPICore;
using FinanceAPICore.DataService;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace FinanceAPIMongoDataService.DataService
{
	public class AccountDataService : IAccountDataService
	{
		string databaseName = "finance";
		string tableName = "accounts";

		public Account GetAccountById(string accountId, string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			Account account = database.LoadRecordById<Account>(tableName, accountId);
			if (account != null && account.ClientID == clientId)
				return account;

			return null;
		}

		public bool InsertAccount(Account account)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			return database.InsertRecord(tableName, account);
		}

		public bool UpdateAccount(Account account)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			var filter = Builders<Account>.Filter.Eq("ClientID", account.ClientID);
			if (account.AvailableBalance == null || account.PendingBalance == null)
			{
				var update = Builders<Account>.Update.Set(nameof(account.AccountName), account.AccountName);
				if(account.AvailableBalance != null)
					update = update.Set(nameof(account.AvailableBalance), account.AvailableBalance);
				if (account.PendingBalance != null)
					update = update.Set(nameof(account.PendingBalance), account.PendingBalance);

				return database.UpdateRecordFields(tableName, account.ID, update, filter);
			}
			return database.UpdateRecord(tableName, account, account.ID, filter);
		}

		public bool DeleteAccount(string accountId, string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			var filter = Builders<Account>.Filter.Eq("ClientID", clientId);
			return database.DeleteRecord(tableName, accountId, filter);
		}

		public decimal GetAccountBalance(string accountId, string clientId)
		{
			throw new NotImplementedException();
		}

		public List<Account> GetAccounts(string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			var filter = Builders<Account>.Filter.Eq("ClientID", clientId);
			return database.LoadRecordsByFilter(tableName, filter);
		}
	}
}
