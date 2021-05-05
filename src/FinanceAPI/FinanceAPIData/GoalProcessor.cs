using System;
using System.Collections.Generic;
using FinanceAPICore;
using FinanceAPICore.DataService;

namespace FinanceAPIData
{
    public class GoalProcessor
    {
        private IGoalDataService _goalDataService;

        public GoalProcessor(IGoalDataService goalDataService)
        {
            _goalDataService = goalDataService;
        }
        
        public bool InsertGoal(Goal goal)
        {
            Goal.Validate(goal);
            return _goalDataService.InsertGoal(goal);
        }

        public bool UpdateGoal(Goal goal)
        {
            Goal.Validate(goal);
            if (goal.Id == null)
                throw new ArgumentException("Goal Id is required", nameof(goal.Id));
            
            return _goalDataService.UpdateGoal(goal);
        }

        public List<Goal> GetGoals(string clientId)
        {
            if(string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId), "clientId is required");

            return _goalDataService.GetGoals(clientId);
        }

        public Goal GetGoalById(string goalId, string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId), "clientId is required");

            if (string.IsNullOrEmpty(goalId))
                throw new ArgumentNullException(nameof(goalId), "goalId is required");

            return _goalDataService.GetGoalById(goalId, clientId);
        }

        public bool DeleteGoal(string goalId, string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId), "clientId is required");

            if (string.IsNullOrEmpty(goalId))
                throw new ArgumentNullException(nameof(goalId), "goalId is required");

            return _goalDataService.DeleteGoal(goalId, clientId);
        }
    }
}