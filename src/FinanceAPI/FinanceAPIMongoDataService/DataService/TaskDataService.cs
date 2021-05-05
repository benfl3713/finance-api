using MongoDB.Driver;
using System.Collections.Generic;
using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using Microsoft.Extensions.Options;

namespace FinanceAPIMongoDataService.DataService
{
	public class TaskDataService : BaseDataService, ITaskDataService
	{
		static string databaseName = "finance";
		static string taskQueueTableName = "task_queue";
		private string _connectionString;

		public TaskDataService(IOptions<AppSettings> appSettings) : base(appSettings)
		{
			_connectionString = appSettings.Value.MongoDB_ConnectionString;
		}

		public bool AddTaskToQueue(Task task)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			return database.InsertRecord(taskQueueTableName, task);
		}

		public bool AllocateTask(string taskId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			var update = Builders<Task>.Update.Set(t => t.Allocated, true);
			return database.UpdateRecordFields(taskQueueTableName, taskId, update);
		}

		public List<Task> GetAllUnAllocatedTasks()
		{
			var filter = Builders<Task>.Filter.Eq(nameof(Task.Allocated), false);
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			return database.LoadRecordsByFilter(taskQueueTableName, filter);
		}

		public bool RemoveTask(string taskId)
		{
			MongoDatabase database = new MongoDatabase(databaseName, _connectionString);
			return database.DeleteRecord<Task>(taskQueueTableName, taskId);
		}
	}
}
