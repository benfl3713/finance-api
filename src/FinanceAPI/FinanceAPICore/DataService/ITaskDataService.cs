using System.Collections.Generic;
using FinanceAPICore.Tasks;

namespace FinanceAPICore.DataService
{
	public interface ITaskDataService
	{
		public bool AddTaskToQueue(Task task);
		public List<Task> GetAllUnAllocatedTasks();
		public bool AllocateTask(string taskId);
		public bool RemoveTask(string taskId);
	}
}
