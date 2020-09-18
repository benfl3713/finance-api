using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using FinanceAPIData.Tasks;
using Hangfire;

namespace FinanceAPIData
{
	public class TaskProcessor
	{
		ITaskDataService _taskDataService;
		private IBackgroundJobClient _backgroundJobs;
		string _connectionString;

		public TaskProcessor(string connectionString, IBackgroundJobClient backgroundJobs)
		{
			_connectionString = connectionString;
			_backgroundJobs = backgroundJobs;
			_taskDataService = new FinanceAPIMongoDataService.DataService.TaskDataService(_connectionString);
		}

		public bool RefreshAccount(string clientId, string accountId)
		{
			// Check if client owns account
			Account account = new AccountProcessor(_connectionString).GetAccountById(accountId, clientId);
			if (account == null)
				return false;

			Task task = new Task($"Account Refresh [{account.AccountName}]", clientId, TaskType.AccountRefresh);
			task.Data.Add("AccountID", account.ID);
			
			var args = new Dictionary<string, object> {{"AccountID", account.ID}};
			_backgroundJobs.Enqueue<AccountRefresh>(t => t.Execute(task));
			return true;
			// return _taskDataService.AddTaskToQueue(task);
		}
	}
}
