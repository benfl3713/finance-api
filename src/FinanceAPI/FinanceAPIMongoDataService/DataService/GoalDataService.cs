using System.Collections.Generic;
using System.Linq;
using FinanceAPICore;
using FinanceAPICore.DataService;
using MongoDB.Driver;

namespace FinanceAPIMongoDataService.DataService
{
    public class GoalDataService : IGoalDataService
    {
        private string connectionString;
        public static string databaseName = "finance";
        public static string tableName = "goals";
        public GoalDataService(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public bool InsertGoal(Goal goal)
        {
            MongoDatabase database = new MongoDatabase(databaseName, connectionString);
            return database.InsertRecord(tableName, goal);
        }

        public bool UpdateGoal(Goal goal)
        {
            MongoDatabase database = new MongoDatabase(databaseName, connectionString);
            return database.UpdateRecord(tableName, goal, goal.Id, idField: nameof(goal.Id));
        }

        public bool DeleteGoal(string id, string clientId)
        {
            MongoDatabase database = new MongoDatabase(databaseName, connectionString);
            var filter = Builders<Goal>.Filter.Eq(nameof(Goal.ClientId), clientId);
            return database.DeleteRecord(tableName, id, filter, nameof(Goal.Id));
        }

        public List<Goal> GetGoals(string clientId)
        {
            MongoDatabase database = new MongoDatabase(databaseName, connectionString);
            var filter = Builders<Goal>.Filter.Eq(nameof(Goal.ClientId), clientId);
            List<Goal> goals = database.LoadRecordsByFilter(tableName, filter);
            
            foreach (IGrouping<string,Goal> accountGoals in goals.GroupBy(g => g.AccountId))
            {
                Account account = new AccountDataService(connectionString).GetAccountById(accountGoals.Key, clientId);
                if(account == null)
                    continue;
                
                foreach (Goal accountGoal in accountGoals)
                {
                    accountGoal.CurrentAmount = account.CurrentBalance;
                    accountGoal.AccountName = account.AccountName;
                }
            }

            return goals;
        }

        public Goal GetGoalById(string goalId, string clientId)
        {
            MongoDatabase database = new MongoDatabase(databaseName, connectionString);
            Goal goal = database.LoadRecordById<Goal>(tableName, goalId, nameof(Goal.Id));
            Account account = new AccountDataService(connectionString).GetAccountById(goal.AccountId, clientId);
            goal.CurrentAmount = account?.CurrentBalance;
            goal.AccountName = account?.AccountName;
            return goal.ClientId != clientId ? null : goal;
        }
    }
}