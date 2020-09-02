using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace FinanceAPICore
{
    public class Account
    {
        [BsonId]
        public string ID;
		[JsonIgnore]
        public string ClientID;
        public string AccountName;
        [BsonIgnore]
        public decimal? CurrentBalance;
        [BsonIgnore]
        public decimal? AvailableBalance;

        public Account()
		{
		}
        public Account(string ID,string AccountName)
        {
            this.ID = ID;
            this.AccountName = AccountName;
        }

        public Account(string AccountName)
        {
            this.AccountName = AccountName;
        }

        public static Account CreateFromJson(JObject jAccount, string clientId)
        {
            Account account = new Account();
            account.ID = jAccount["ID"]?.ToString();
            account.AccountName = jAccount["AccountName"]?.ToString();
            account.ClientID = clientId;
            return account;
        }
    }
}
