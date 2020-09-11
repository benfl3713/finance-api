using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIData
{
	public class TaskProcessor
	{
		ITaskDataService _taskDataService;
		string _connectionString;

		public TaskProcessor(string connectionString)
		{
			_connectionString = connectionString;
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

			return _taskDataService.AddTaskToQueue(task);
		}
	}
}
