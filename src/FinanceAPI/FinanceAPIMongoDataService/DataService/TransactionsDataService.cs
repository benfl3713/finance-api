using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FinanceAPICore;
using FinanceAPICore.DataService;
using MongoDB.Driver;

namespace FinanceAPIMongoDataService.DataService
{
	public class TransactionsDataService : ITransactionsDataService
	{
		public static string databaseName = "finance";
		public static string tableName = "transactions";
		string connectionString;

		public TransactionsDataService(string connectionString)
		{
			this.connectionString = connectionString;
		}

		public bool DeleteAllAccountTransactions(string accountId, string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var filter = Builders<Transaction>.Filter.Eq(t => t.ClientID, clientId) & Builders<Transaction>.Filter.Eq(t => t.AccountID, accountId);
			return database.DeleteManyRecords(tableName, filter);
		}

		public bool DeleteTransaction(string transactionId, string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var filter = Builders<Transaction>.Filter.Eq("ClientID", clientId);
			return database.DeleteRecord<Transaction>(tableName, transactionId, filter);
		}

		public Transaction GetTransactionById(string transactionId, string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var transaction = database.LoadRecordById<Transaction>(tableName, transactionId);
			if (transaction != null && transaction.ClientID == clientId)
				return transaction;

			return null;
		}

		public List<Transaction> GetTransactions(string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var filter = Builders<Transaction>.Filter.Eq("ClientID", clientId);
			return database.LoadRecordsByFilter(tableName, filter).OrderByDescending(t => t.Date).ToList();
		}

		public bool ImportDatafeedTransaction(Transaction transaction)
		{
			Transaction existingTransaction = GetTransactionById(transaction.ID, transaction.ClientID);
			if(existingTransaction != null)
			{
				if (existingTransaction.Owner == "User")
					return false;
				
				if(string.IsNullOrEmpty(transaction.Logo))
					transaction.Logo = existingTransaction.Logo;
				return UpdateTransaction(transaction);
			}

			return InsertTransaction(transaction);
		}

		public bool InsertTransaction(Transaction transaction)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			return database.InsertRecord(tableName, transaction);
		}

		public bool UpdateTransaction(Transaction transaction)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var filter = Builders<Transaction>.Filter.Eq("ClientID", transaction.ClientID);
			return database.UpdateRecord(tableName, transaction, transaction.ID, filter);
		}

		public bool UpdateTransactionLogo(string transactionId, string logo)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var update = Builders<Transaction>.Update.Set(t => t.Logo, logo);
			var filter = Builders<Transaction>.Filter.Eq(t => t.ID, transactionId);

			return database.PartialUpdateRecord(tableName, update, filter);
		}
	}
}
