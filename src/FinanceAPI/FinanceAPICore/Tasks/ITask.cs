using System;

namespace FinanceAPICore.Tasks
{
	public interface ITask
	{
		public void Execute(Task task);
		event EventHandler Completed;
	}
}
