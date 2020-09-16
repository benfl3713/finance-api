using FinanceAPICore.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using Serilog.Events;

namespace FinanceAPIData.Tasks
{
	public class BaseTask : ITask
	{
		public Task Task { get; set; }
		public virtual event EventHandler Completed;

		public virtual void Execute(Dictionary<string, object> args, TaskSettings settings)
		{
			Log($"{Task.Name} has finished");
			Completed?.Invoke(this, new EventArgs());
		}

		protected void Log(string message, LogEventLevel eventLevel = LogEventLevel.Information)
		{
			var logger = Serilog.Log.Logger;
			logger?.Write(eventLevel, message);
		}
	}
}
