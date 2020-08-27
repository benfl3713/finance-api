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
		string databaseName = "finance";
		string tableName = "transactions";

		public bool DeleteTransaction(string transactionId)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			return database.DeleteRecord<Transaction>(tableName, transactionId);
		}

		public Transaction GetTransactionById(string transactionId)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			return database.LoadRecordById<Transaction>(tableName, transactionId);
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
			return database.UpdateRecord(tableName, transaction, transaction.ID);
		}
	}
}
