﻿using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FinanceAPICore
{
    public class AccountSettings
    {
        [BsonId]
        [Required]
        public string AccountID;

        [JsonConverter(typeof(StringEnumConverter))]
        public RefreshIntervals RefreshInterval = RefreshIntervals.Never;
        public bool GenerateAdjustments = true;
        
        public enum RefreshIntervals
        {
            Never,
            hourly,
            sixHours,
            biDaily,
            Daily
        }
    }
}