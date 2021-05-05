using System.Collections.Generic;
using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using FinanceAPIData.Tasks;
using Hangfire;

namespace FinanceAPIData
{
	public class TaskProcessor
	{
		ITaskDataService _taskDataService;
		private IBackgroundJobClient _backgroundJobs;
		private readonly AccountProcessor _accountProcessor;

		public TaskProcessor(ITaskDataService taskDataService, IBackgroundJobClient backgroundJobs, AccountProcessor accountProcessor)
		{
			_backgroundJobs = backgroundJobs;
			_taskDataService = taskDataService;
			_accountProcessor = accountProcessor;
		}

		public bool RefreshAccount(string clientId, string accountId)
		{
			// Check if client owns account
			Account account = _accountProcessor.GetAccountById(accountId, clientId);
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
