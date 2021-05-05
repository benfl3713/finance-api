using System;
using System.Collections.Generic;
using System.Linq;
using FinanceAPICore;
using FinanceAPICore.DataService;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FinanceAPIMongoDataService.DataService
{
	public class TransactionsDataService : BaseDataService, ITransactionsDataService
	{
		public static string databaseName = "finance";
		public static string tableName = "transactions";
		private readonly string _connectionString;

		public TransactionsDataService(IOptions<AppSettings> appSettings) : base(appSettings)
		{
			_connectionString = appSettings.Value.MongoDB_ConnectionString;
		}

		public bool DeleteAllAccountTransactions(string accountId, string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			var filter = Builders<Transaction>.Filter.Eq(t => t.ClientID, clientId) & Builders<Transaction>.Filter.Eq(t => t.AccountID, accountId);
			return database.DeleteManyRecords(tableName, filter);
		}

		public bool DeleteTransaction(string transactionId, string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			var filter = Builders<Transaction>.Filter.Eq("ClientID", clientId);
			return database.DeleteRecord<Transaction>(tableName, transactionId, filter);
		}

		public bool DeleteOldTransaction(string transactionId, string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			var filter = Builders<OldTransaction>.Filter.Eq("ClientID", clientId);
			return database.DeleteRecord(tableName, transactionId, filter);
		}

		public Transaction GetTransactionById(string transactionId, string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			var transaction = database.LoadRecordById<Transaction>(tableName, transactionId);
			if (transaction != null && transaction.ClientID == clientId)
				return transaction;

			return null;
		}

		public List<Transaction> GetTransactions(string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			var filter = Builders<Transaction>.Filter.Eq("ClientID", clientId);
			return database.LoadRecordsByFilter(tableName, filter).OrderByDescending(t => t.Date).ToList();
		}

		public List<BsonDocument> GetOldTransactions(string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			var filter = Builders<BsonDocument>.Filter.Eq("ClientID", clientId);
			return database.LoadRecordsByFilter(tableName, filter).OrderByDescending(t => DateTime.Parse(t["Date"].ToString())).ToList();
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
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			return database.InsertRecord(tableName, transaction);
		}

		public bool UpdateTransaction(Transaction transaction)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			var filter = Builders<Transaction>.Filter.Eq("ClientID", transaction.ClientID);
			return database.UpdateRecord(tableName, transaction, transaction.ID, filter);
		}

		public bool UpdateTransactionLogo(string transactionId, string logo)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			var update = Builders<Transaction>.Update.Set(t => t.Logo, logo);
			var filter = Builders<Transaction>.Filter.Eq(t => t.ID, transactionId);

			return database.PartialUpdateRecord(tableName, update, filter);
		}
	}
}
