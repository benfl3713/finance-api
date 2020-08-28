using FinanceAPICore;
using FinanceAPICore.DataService;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FinanceAPIMongoDataService.DataService
{
	public class ClientDataService : IClientDataService
	{
		string databaseName = "finance";
		string tableName = "clients";

		public bool InsertClient(Client client)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			return database.InsertRecord(tableName, client);
		}

		public Client GetClientById(string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			return database.LoadRecordById<Client>(tableName, clientId);
		}

		public bool UpdateClient(Client client)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			return database.UpdateRecord(tableName, client, client.ID);
		}

		public bool DeleteClient(string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			return database.DeleteRecord<Client>(tableName, clientId);
		}

		public Client GetClientByUsername(string username)
		{
			MongoDatabase database = new MongoDatabase(databaseName);
			var filter = Builders<Client>.Filter.Eq("Username", username);
			List<Client> records = database.LoadRecordsByFilter(tableName, filter);
			if (records.Any())
				return records.First();

			return null;
		}
	}
}
