using MongoDB.Bson.Serialization.Attributes;
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
        public Client() { }
        public Client(string id, string firstname, string lastname, string username)
        {
            ID = id;
            FirstName = firstname;
            LastName = lastname;
            Username = username;
        }
        public Client(string firstname, string lastname, string username)
        {
            FirstName = firstname;
            LastName = lastname;
            Username = username;
        }

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
