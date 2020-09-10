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
		ITaskDataService _taskDataService = new FinanceAPIMongoDataService.DataService.TaskDataService();

		public bool RefreshAccount(string clientId, string accountId)
		{
			// Check if client owns account
			Account account = new AccountProcessor().GetAccountById(accountId, clientId);
			if (account == null)
				return false;

			Task task = new Task($"Account Refresh [{account.AccountName}]", clientId, TaskType.AccountRefresh);
			task.Data.Add("AccountID", account.ID);

			return _taskDataService.AddTaskToQueue(task);
		}
	}
}
