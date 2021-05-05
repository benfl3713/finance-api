using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FinanceAPICore.Tasks
{
	public class Task
	{
		[BsonId]
		public string ID { get; set; }
		public string Name { get; set; }
		public string ClientID { get; set; }
		[JsonConverter(typeof(StringEnumConverter))]
		[BsonRepresentation(BsonType.String)]
		public TaskType TaskType { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime? StartedDate { get; set; }
		public DateTime? FinishedDate { get; set; }
		public bool Allocated { get; set; } = false;
		public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

		public Task(){}

		public Task(string name, string clientId, TaskType taskType, DateTime? createdDate = null)
		{
			ID = Guid.NewGuid().ToString();
			Name = name;
			ClientID = clientId;
			TaskType = taskType;
			CreatedDate = createdDate ?? DateTime.Now;
		}
	}

	public enum TaskType
	{
		AccountRefresh,
		LogoCalculator,
		DemoClearDownTask
	}
}
