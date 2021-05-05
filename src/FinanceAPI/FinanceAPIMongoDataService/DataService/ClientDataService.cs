using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using FinanceAPICore;
using FinanceAPICore.DataService;
using Microsoft.Extensions.Options;

namespace FinanceAPIMongoDataService.DataService
{
	public class ClientDataService : BaseDataService, IClientDataService
	{
		string databaseName = "finance";
		string tableName = "clients";
		private readonly string _connectionString;

		public ClientDataService(IOptions<AppSettings> appOptions): base(appOptions)
		{
			_connectionString = appOptions.Value.MongoDB_ConnectionString;
		}

		public bool InsertClient(Client client)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			return database.InsertRecord(tableName, client);
		}

		public Client GetClientById(string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			return database.LoadRecordById<Client>(tableName, clientId);
		}

		public bool UpdateClient(Client client)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			return database.UpdateRecord(tableName, client, client.ID);
		}

		public bool DeleteClient(string clientId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			return database.DeleteRecord<Client>(tableName, clientId);
		}

		public Client GetClientByUsername(string username)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			var filter = Builders<Client>.Filter.Eq("Username", username);
			List<Client> records = database.LoadRecordsByFilter(tableName, filter);
			if (records.Any())
				return records.First();

			return null;
		}

		public List<Client> GetAllClients()
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			return database.LoadRecordsByFilter<Client>(tableName, null);
		}
	}
}
