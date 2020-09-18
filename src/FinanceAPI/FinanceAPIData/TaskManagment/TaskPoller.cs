using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FinanceAPIData.Tasks;
using Hangfire;
using Hangfire.States;
using Hangfire.Storage;

namespace FinanceAPIData.TaskManagment
{
	public class TaskPoller
	{
		private TaskSettings _taskSettings;
		private bool _isCanceled;
		private ITaskDataService _taskDataService;
		private TaskFactory taskFactory;

		public TaskPoller(IOptions<TaskSettings> taskSettings, IBackgroundJobClient backgroundJobs, TransactionLogoCalculator transactionLogoCalculator)
		{
			_taskSettings = taskSettings.Value;
			taskFactory = new TaskFactory(backgroundJobs, transactionLogoCalculator);

			_taskDataService = new FinanceAPIMongoDataService.DataService.TaskDataService(_taskSettings.MongoDB_ConnectionString);

			System.Threading.Tasks.Task threadedTask = new System.Threading.Tasks.Task(() => Start());
			threadedTask.Start();


			RecurringJob.AddOrUpdate(() => transactionLogoCalculator.Run(null, null), Cron.Hourly);
		}

		public void Start()
		{
			while (!_isCanceled)
			{
				//Do Poll
				List<Task> queue = _taskDataService.GetAllUnAllocatedTasks();
				foreach (Task task in queue)
				{
					taskFactory.StartTask(task, _taskSettings);
				}
				
				Thread.Sleep(_taskSettings.PollingInterval);
			}
		}

		public void Stop()
		{
			_isCanceled = true;
		}
	}
}
