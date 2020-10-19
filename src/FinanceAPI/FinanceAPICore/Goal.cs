using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;

namespace FinanceAPICore
{
    public class Goal
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string AccountId { get; set; }
        [BsonIgnore]
        public string AccountName { get; set; }
        public string ClientId { get; set; }
        public DateTime Date { get; set; }
        public decimal GoalAmount { get; set; }
        [BsonIgnore]
        public decimal? CurrentAmount { get; set; }

        public static void Validate(Goal goal)
        {
            if (goal == null)
                throw new ArgumentNullException(nameof(goal));
            if (string.IsNullOrEmpty(goal.ClientId))
                throw new ArgumentNullException(nameof(goal.ClientId));
            if (string.IsNullOrEmpty(goal.AccountId))
                throw new ArgumentNullException(nameof(goal.AccountId));
        }

        public static Goal CreateFromJson(JObject jGoal, string clientId)
        {
            Goal goal = new Goal();
            goal.Id = jGoal["Id"]?.ToString();
            goal.Name = jGoal["Name"]?.ToString();
            goal.Date = DateTime.Parse(jGoal["Date"]?.ToString());
            goal.AccountId = jGoal["AccountId"]?.ToString();
            goal.GoalAmount = decimal.Parse(jGoal["GoalAmount"]?.ToString());
            goal.ClientId = clientId;
            return goal;
        }
    }
}