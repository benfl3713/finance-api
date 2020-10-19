using System.Collections.Generic;

namespace FinanceAPICore.DataService
{
    public interface IGoalDataService
    {
        public bool InsertGoal(Goal goal);
        public bool UpdateGoal(Goal goal);
        public bool DeleteGoal(string id, string clientId);
        public List<Goal> GetGoals(string clientId);
        public Goal GetGoalById(string goalId, string clientId);
    }
}