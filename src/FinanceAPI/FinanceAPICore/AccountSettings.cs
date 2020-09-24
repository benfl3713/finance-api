using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanceAPICore
{
    public class AccountSettings
    {
        [BsonId]
        [Required]
        public string AccountID;
        public RefreshIntervals RefreshInterval = RefreshIntervals.Never;
        public bool GenerateAdjustments = true;
        
        public enum RefreshIntervals
        {
            Never,
            hourly,
            biDaily,
            Daily
        }
    }
}