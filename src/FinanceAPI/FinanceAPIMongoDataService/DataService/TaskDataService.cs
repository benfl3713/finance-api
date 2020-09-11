using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIMongoDataService.DataService
{
	public class TaskDataService : ITaskDataService
	{
		static string databaseName = "finance";
		static string taskQueueTableName = "task_queue";
		string connectionString;

		public TaskDataService(string connectionString)
		{
			this.connectionString = connectionString;
		}

		public bool AddTaskToQueue(Task task)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			return database.InsertRecord(taskQueueTableName, task);
		}

		public bool AllocateTask(string taskId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			var update = Builders<Task>.Update.Set(t => t.Allocated, true);
			return database.UpdateRecordFields(taskQueueTableName, taskId, update);
		}

		public List<Task> GetAllUnAllocatedTasks()
		{
			var filter = Builders<Task>.Filter.Eq(nameof(Task.Allocated), false);
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			return database.LoadRecordsByFilter(taskQueueTableName, filter);
		}

		public bool RemoveTask(string taskId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, connectionString);
			return database.DeleteRecord<Task>(taskQueueTableName, taskId);
		}
	}
}
