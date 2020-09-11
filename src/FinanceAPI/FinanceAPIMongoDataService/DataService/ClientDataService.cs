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
		string connectionString;

		public ClientDataService(string connectionString)
		{
			this.connectionString = connectionString;
		}

		public bool InsertClient(Client client)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			return database.InsertRecord(tableName, client);
		}

		public Client GetClientById(string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			return database.LoadRecordById<Client>(tableName, clientId);
		}

		public bool UpdateClient(Client client)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			return database.UpdateRecord(tableName, client, client.ID);
		}

		public bool DeleteClient(string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			return database.DeleteRecord<Client>(tableName, clientId);
		}

		public Client GetClientByUsername(string username)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var filter = Builders<Client>.Filter.Eq("Username", username);
			List<Client> records = database.LoadRecordsByFilter(tableName, filter);
			if (records.Any())
				return records.First();

			return null;
		}

		public List<Client> GetAllClients()
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			return database.LoadRecordsByFilter<Client>(tableName, null);
		}
	}
}
