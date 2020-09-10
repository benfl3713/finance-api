﻿using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FinanceAPIData.TaskManagment
{
	public class TaskPoller
	{
		private TaskSettings _taskSettings;
		private bool IsCanceled;
		private ITaskDataService _taskDataService = new FinanceAPIMongoDataService.DataService.TaskDataService();
		private TaskFactory taskFactory;

		public TaskPoller(IOptions<TaskSettings> taskSettings)
		{
			_taskSettings = taskSettings.Value;
			taskFactory = new TaskFactory();

			System.Threading.Tasks.Task threadedTask = new System.Threading.Tasks.Task(() => Start());
			threadedTask.Start();
		}

		public void Start()
		{
			while (!IsCanceled)
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
			IsCanceled = true;
		}
	}
}