using System;
using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using Hangfire;

namespace FinanceAPIData.TaskManagment
{
	public class TaskFactory
	{
		private ITaskDataService _taskDataService;
		private TransactionLogoCalculator _transactionLogoCalculator;

		public TaskFactory(IBackgroundJobClient backgroundJobs, TransactionLogoCalculator transactionLogoCalculator, ITaskDataService taskDataService)
		{
			_transactionLogoCalculator = transactionLogoCalculator;
			_taskDataService = taskDataService;
		}

		public void StartTask(Task task, TaskSettings taskSettings)
		{
			if (!_taskDataService.AllocateTask(task.ID))
				return;

			ITask taskInstance = GetInstance(task.TaskType, taskSettings);
			if (taskInstance == null)
				return;

			taskInstance.Completed += Complete;

			Serilog.Log.Logger?.Information($"Starting task: {task.Name}");

			try
			{
				// Executes task in the background
				// System.Threading.Tasks.Task threadedTask = new System.Threading.Tasks.Task(() =>
				// {
				// 	taskInstance.Execute(task.Data, taskSettings);
				// });
				// threadedTask.Start();
				
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger?.Error(ex.Message);
			}
		}

		private void Complete(object sender, EventArgs args)
		{
			//Remove From Queue
			// _taskDataService.RemoveTask((sender as ITask)?.Task.ID);
		}

		private ITask GetInstance(TaskType taskType, TaskSettings taskSettings)
		{
			switch (taskType)
			{
				case TaskType.AccountRefresh:
					//return new AccountRefresh(new OptionsWrapper<TaskSettings>(taskSettings), );
				default:
					return null;
			}
		}
	}
}
