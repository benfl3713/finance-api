using System;
using FinanceAPICore.Tasks;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

namespace FinanceAPIData.Tasks
{
	public abstract class BaseTask : ITask
	{
		private readonly ILogger _logger;
		public BaseTask(IOptions<TaskSettings> settings)
		{
			_logger = Serilog.Log.ForContext(GetType());
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
			_logger.Write(eventLevel, $"[{GetType().Name}] {message}");
		}
	}
}
