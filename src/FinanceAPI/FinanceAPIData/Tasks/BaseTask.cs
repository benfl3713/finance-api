using FinanceAPICore.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.Extensions.Options;
using Serilog.Events;

namespace FinanceAPIData.Tasks
{
	public class BaseTask : ITask
	{
		public BaseTask(IOptions<TaskSettings> settings)
		{
			Settings = settings.Value;
		}
		public virtual event EventHandler Completed;
		protected TaskSettings Settings;

		public virtual void Execute(Task task)
		{
			Log($"{task.Name} has finished");
			Completed?.Invoke(this, new EventArgs());
		}

		protected void Log(string message, LogEventLevel eventLevel = LogEventLevel.Information)
		{
			Serilog.Log.Logger?.Write(eventLevel, message);
		}
	}
}
