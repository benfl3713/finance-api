using System;
using System.Collections.Generic;
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

		public bool DeleteTransaction(string transactionId, string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			var filter = Builders<Transaction>.Filter.Eq("ClientID", clientId);
			return database.DeleteRecord<Transaction>(tableName, transactionId, filter);
		}

		public Transaction GetTransactionById(string transactionId, string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			var transaction = database.LoadRecordById<Transaction>(tableName, transactionId);
			if (transaction != null && transaction.ClientID == clientId)
				return transaction;

			return null;
		}

		public List<Transaction> GetTransactions(string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			var filter = Builders<Transaction>.Filter.Eq("ClientID", clientId);
			return database.LoadRecordsByFilter(tableName, filter);
		}

		public bool InsertTransaction(Transaction transaction)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			return database.InsertRecord(tableName, transaction);
		}

		public bool UpdateTransaction(Transaction transaction)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			var filter = Builders<Transaction>.Filter.Eq("ClientID", transaction.ClientID);
			return database.UpdateRecord(tableName, transaction, transaction.ID, filter);
		}
	}
}
