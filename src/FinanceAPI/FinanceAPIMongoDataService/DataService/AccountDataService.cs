using FinanceAPICore;
using FinanceAPICore.DataService;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIMongoDataService.DataService
{
	public class AccountDataService : IAccountDataService
	{
		string databaseName = "finance";
		string tableName = "accounts";

		public Account GetAccountById(string accountId)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			return database.LoadRecordById<Account>(tableName, accountId);
		}

		public bool InsertAccount(Account account)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			return database.InsertRecord(tableName, account);
		}

		public bool UpdateAccount(Account account)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			if(account.AvailableBalance == null || account.PendingBalance == null)
			{
				var update = Builders<Account>.Update.Set(nameof(account.AccountName), account.AccountName);
				if(account.AvailableBalance != null)
					update = update.Set(nameof(account.AvailableBalance), account.AvailableBalance);
				if (account.PendingBalance != null)
					update = update.Set(nameof(account.PendingBalance), account.PendingBalance);

				return database.UpdateRecordFields(tableName, account.ID, update);
			}
			return database.UpdateRecord(tableName, account, account.ID);
		}

		public bool DeleteAccount(string accountId)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			return database.DeleteRecord<Account>(tableName, accountId);
		}

		public decimal GetAccountBalance(string accountId)
		{
			throw new NotImplementedException();
		}
	}
}
