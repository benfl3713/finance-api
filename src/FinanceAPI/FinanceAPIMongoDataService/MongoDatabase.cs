using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FinanceAPIMongoDataService
{
	internal class MongoDatabase
	{
		private IMongoDatabase db;
		public MongoDatabase(string database, string connectionString)
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

		public T LoadRecordById<T>(string table, string id, string idField = "ID")
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				var filter = Builders<T>.Filter.Eq(idField, id);
				return collection.Find(filter).FirstOrDefault();
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
				if (filter == null)
					return collection.Find(Builders<T>.Filter.Empty).ToList();
				return collection.Find(filter).ToList();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return default;
			}
		}

		public bool UpdateRecord<T>(string table, T record, string id, FilterDefinition<T> customFilter = null, string idField = "ID")
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				var filter = Builders<T>.Filter.Eq(idField, id);
				if (customFilter != null)
					filter &= customFilter;
				collection.ReplaceOne(filter, record);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}

		public bool PartialUpdateRecord<T>(string table, UpdateDefinition<T> updateDefinition, FilterDefinition<T> filter = null)
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				collection.UpdateOne(filter, updateDefinition);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}

		public bool UpsertRecord<T>(string table, T record, string id, FilterDefinition<T> customFilter = null, string idField = "ID")
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				var filter = Builders<T>.Filter.Eq(idField, id);
				if (customFilter != null)
					filter &= customFilter;
				var options = new ReplaceOptions { IsUpsert = true };
				collection.ReplaceOne(filter, record, options);
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
					filter &= customFilter;
				collection.UpdateOne(filter, updateDefinition);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}

		public bool DeleteRecord<T>(string table, string id, FilterDefinition<T> customFilter = null, string idField = "ID")
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				var filter = Builders<T>.Filter.Eq(idField, id);
				if (customFilter != null)
					filter &= customFilter;
				var result = collection.DeleteOne(filter);
				return result.DeletedCount > 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}

		public bool DeleteManyRecords<T>(string table, FilterDefinition<T> filter)
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				var result = collection.DeleteMany(filter);
				return result.IsAcknowledged;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}

		public decimal GetSumOfFields<T>(string table, Expression<Func<T, decimal>> sumFieldSelector, Expression<Func<T, bool>> filter)
		{
			try
			{
				var collection = db.GetCollection<T>(table);
				return collection.AsQueryable().Where(filter).Select(sumFieldSelector).ToList().Sum();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return default;
			}
		}
	}
}
