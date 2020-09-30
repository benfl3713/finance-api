using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPICore
{
    public class Client
    {
        [BsonId]
        public string ID;
        public string FirstName;
        public string LastName;
        public string Username;
        [JsonIgnore]
        public string Password;
        public Client() { }

        public static Client CreateFromJson(JObject jClient)
		{
            Client client = new Client();
            client.ID = jClient["ID"]?.ToString();
            client.FirstName = jClient["FirstName"]?.ToString();
            client.LastName = jClient["LastName"]?.ToString();
            client.Username = jClient["Username"]?.ToString();

            return client;
        }
    }
}
