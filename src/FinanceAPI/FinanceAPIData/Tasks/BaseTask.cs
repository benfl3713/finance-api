using FinanceAPICore.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIData.Tasks
{
	public class BaseTask : ITask
	{
		public Task Task { get; set; }
		public virtual event EventHandler Completed;

		public virtual void Execute(Dictionary<string, object> args, TaskSettings settings)
		{
			Console.WriteLine($"{Task.Name} has finished");
			Completed.Invoke(this, new EventArgs());
		}
	}
}
