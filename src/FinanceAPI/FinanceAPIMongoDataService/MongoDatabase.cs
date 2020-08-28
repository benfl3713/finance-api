using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIMongoDataService
{
	internal class MongoDatabase
	{
		private IMongoDatabase db;
		public MongoDatabase(string database, string connectionString = "mongodb://bendrive")
		{
			var client = new MongoClient(connectionString);
			db = client.GetDatabase(database);
		}

		public bool InsertRecord<T>(string table, T record)
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				collection.InsertOne(record);
				return true;
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}

		public T LoadRecordById<T>(string table, string id)
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				var filter = Builders<T>.Filter.Eq("ID", id);
				return collection.Find(filter).First();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return default;
			}
		}

		public List<T> LoadRecordsByFilter<T>(string table, FilterDefinition<T> filter)
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				return collection.Find(filter).ToList();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return default;
			}
		}

		public bool UpdateRecord<T>(string table, T record, string id, FilterDefinition<T> customFilter = null)
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				var filter = Builders<T>.Filter.Eq("ID", id);
				if (customFilter != null)
					filter = filter & customFilter;
				collection.ReplaceOne(filter, record);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}

		public bool UpdateRecordFields<T>(string table, string id, UpdateDefinition<T> updateDefinition, FilterDefinition<T> customFilter = null)
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				var filter = Builders<T>.Filter.Eq("ID", id);
				if (customFilter != null)
					filter = filter & customFilter;
				collection.UpdateOne(filter, updateDefinition);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}

		public bool DeleteRecord<T>(string table, string id, FilterDefinition<T> customFilter = null)
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				var filter = Builders<T>.Filter.Eq("ID", id);
				if (customFilter != null)
					filter = filter & customFilter;
				var result = collection.DeleteOne(filter);
				return result.DeletedCount > 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}
	}
}
