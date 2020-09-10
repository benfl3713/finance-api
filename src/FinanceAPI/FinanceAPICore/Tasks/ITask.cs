using FinanceAPICore.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIData.Tasks
{
	public interface ITask
	{
		public void Execute(Dictionary<string, object> args, TaskSettings settings);
		event EventHandler Completed;
		public Task Task { get; set; }
	}
}
