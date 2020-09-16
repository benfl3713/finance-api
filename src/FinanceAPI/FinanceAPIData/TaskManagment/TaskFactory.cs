using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using FinanceAPIData.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIData.TaskManagment
{
	public class TaskFactory
	{
		private ITaskDataService _taskDataService;
		private TransactionLogoCalculator _transactionLogoCalculator;

		public TaskFactory(TransactionLogoCalculator transactionLogoCalculator)
		{
			_transactionLogoCalculator = transactionLogoCalculator;
		}

		public void StartTask(Task task, TaskSettings taskSettings)
		{
			_taskDataService = new FinanceAPIMongoDataService.DataService.TaskDataService(taskSettings.MongoDB_ConnectionString);

			if (!_taskDataService.AllocateTask(task.ID))
				return;

			ITask taskInstance = GetInstance(task.TaskType);
			if (taskInstance == null)
				return;

			taskInstance.Task = task;

			taskInstance.Completed += Complete;

			Serilog.Log.Logger?.Information($"Starting task: {task.Name}");

			try
			{
				// Executes task in the background
				System.Threading.Tasks.Task threadedTask = new System.Threading.Tasks.Task(() =>
				{
					taskInstance.Execute(task.Data, taskSettings);
				});
				threadedTask.Start();
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger?.Error(ex.Message);
			}
		}

		private void Complete(object sender, EventArgs args)
		{
			//Remove From Queue
			_taskDataService.RemoveTask((sender as ITask)?.Task.ID);

			if (sender is AccountRefresh accountRefresh)
				_transactionLogoCalculator.Run(accountRefresh.Task.ClientID, accountRefresh.Task.Data["AccountID"].ToString());
		}

		private ITask GetInstance(TaskType taskType)
		{
			switch (taskType)
			{
				case TaskType.AccountRefresh:
					return new AccountRefresh();
				default:
					return null;
			}
		}
	}
}
