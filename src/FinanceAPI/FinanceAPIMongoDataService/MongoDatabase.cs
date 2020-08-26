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
		public MongoDatabase(string database)
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

		public bool UpdateRecord<T>(string table, T record, string id)
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				collection.ReplaceOne(new BsonDocument("_id", id), record);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}

		public bool UpdateRecordFields<T>(string table, string id, UpdateDefinition<T> updateDefinition)
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				collection.UpdateOne(new BsonDocument("_id", id), updateDefinition);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}

		public bool DeleteRecord<T>(string table, string id)
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				var filter = Builders<T>.Filter.Eq("ID", id);
				collection.DeleteOne(filter);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}
	}
}
