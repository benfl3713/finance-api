using FinanceAPICore.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIData.Tasks
{
	public interface ITask
	{
		public void Execute(Task task);
		event EventHandler Completed;
	}
}
