using FinanceAPICore;
using FinanceAPICore.DataService;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinanceAPIMongoDataService.DataService
{
	public class AccountDataService : IAccountDataService
	{
		static string databaseName = "finance";
		static string tableName = "accounts";
		string connectionString;

		public AccountDataService(string connectionString)
		{
			this.connectionString = connectionString;
		}

		public Account GetAccountById(string accountId, string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			Account account = database.LoadRecordById<Account>(tableName, accountId);
			if (account != null && account.ClientID == clientId)
			{
				account.CurrentBalance = GetCurrentAccountBalance(account.ID);
				account.AvailableBalance = GetPendingAccountBalance(account.ID);
				return account;
			}

			return null;
		}

		public bool InsertAccount(Account account)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			return database.InsertRecord(tableName, account);
		}

		public bool UpdateAccount(Account account)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var filter = Builders<Account>.Filter.Eq("ClientID", account.ClientID);
			return database.UpdateRecord(tableName, account, account.ID, filter);
		}

		public bool DeleteAccount(string accountId, string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var filter = Builders<Account>.Filter.Eq("ClientID", clientId);
			return database.DeleteRecord(tableName, accountId, filter);
		}

		public List<Account> GetAccounts(string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var filter = Builders<Account>.Filter.Eq("ClientID", clientId);
			var accounts =  database.LoadRecordsByFilter(tableName, filter);
			Parallel.ForEach(accounts, a =>
			{
				a.CurrentBalance = GetCurrentAccountBalance(a.ID);
				a.AvailableBalance = GetPendingAccountBalance(a.ID);
			});

			return accounts;
		}

		private decimal GetCurrentAccountBalance(string accountId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			return database.GetSumOfFields<Transaction>(TransactionsDataService.tableName, t => t.Amount, t => t.AccountID == accountId && t.Status != Status.PENDING);
		}

		private decimal GetPendingAccountBalance(string accountId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			return database.GetSumOfFields<Transaction>(TransactionsDataService.tableName, t => t.Amount, t => t.AccountID == accountId);
		}
	}
}
