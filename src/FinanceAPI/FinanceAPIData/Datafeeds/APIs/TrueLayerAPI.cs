using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Utilities;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceAPIData.Datafeeds.APIs
{
	public class TrueLayerAPI : IDatafeedAPI
	{
        public string _ClientId { private get; set; } = string.Empty;
        public string _Secret { private get; set; } = string.Empty;
        private string _AuthUrl = "https://auth.truelayer-sandbox.com";
        private string _ApiUrl = "https://api.truelayer-sandbox.com";
        public IDatafeedDataService _datafeedDataService;
		private string datafeedName = "TRUELAYER";

		public TrueLayerAPI(){}
		public TrueLayerAPI(string connectionString, string clientId, string clientSecret, string mode)
		{
            _datafeedDataService = new FinanceAPIMongoDataService.DataService.DatafeedDataService(connectionString);
            _ClientId = clientId;
            _Secret = clientSecret;

            SetMode(mode);
		}

        public void SetMode(string mode)
		{
            if (mode == "Live")
            {
                _AuthUrl = "https://auth.truelayer.com";
                _ApiUrl = "https://api.truelayer.com";
            }
			else
			{
                _AuthUrl = "https://auth.truelayer-sandbox.com";
                _ApiUrl = "https://api.truelayer-sandbox.com";
            }
        }

        public bool RegisterNewClient(string publicToken, string clientId, string requestUrl)
        {
            var client = new RestClient($"{_AuthUrl}/connect/token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("undefined", $"grant_type=authorization_code&client_id={_ClientId}&client_secret={_Secret}&redirect_uri={requestUrl}&code={publicToken}", ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                dynamic objContent = JsonConvert.DeserializeObject(response.Content);
                string accessKey = objContent?.access_token;
                string refreshToken = objContent?.refresh_token;
                //Gets info about the provider
                GetProviderInfo(accessKey, out string vendorName, out string vendorID);
                if (!string.IsNullOrEmpty(accessKey))
                {
                    Datafeed datafeed = new Datafeed(clientId, datafeedName, vendorID, vendorName, DateTime.Now, SecurityService.EncryptTripleDES(accessKey), SecurityService.EncryptTripleDES(refreshToken));
                    return _datafeedDataService.AddUpdateClientDatafeed(datafeed);
                }
            }

            return false;
        }

        private bool GetProviderInfo(string accesskey, out string providerName, out string providerId, bool refreshToken = true)
        {
            providerName = null;
            providerId = null;
            try
            {
                var client = new RestClient($"{_ApiUrl}/data/v1/me");
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", $"Bearer {accesskey}");
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    dynamic objContent = JsonConvert.DeserializeObject(response.Content);
                    providerName = objContent.results[0].provider.display_name;
                    providerId = objContent.results[0].provider.provider_id;
                    return true;
                }
                else if (response.Content.Length > 0 && response.StatusCode == System.Net.HttpStatusCode.Unauthorized && refreshToken)
                {
                    return GetProviderInfo(SecurityService.DecryptTripleDES(RefreshTokenExchange(SecurityService.EncryptTripleDES(accesskey))), out providerName, out providerId, false);
                }
            }
            catch (Exception e)
            {
				Console.WriteLine(e.Message);
            }
            return false;
        }

        public List<ExternalAccount> GetExternalAccounts(string clientId, string encryptedAccessKey, string vendorID, string vendorName, string provider, bool refreshToken = true)
        {
            List<ExternalAccount> accounts = new List<ExternalAccount>();
            try
            {
                string accesskey = SecurityService.DecryptTripleDES(encryptedAccessKey);
                var client = new RestClient($"{_ApiUrl}/data/v1/accounts");
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", $"Bearer {accesskey}");
                IRestResponse response = client.Execute(request);

                if (response.Content.Length > 0 && response.StatusCode == System.Net.HttpStatusCode.Unauthorized && refreshToken)
                {
                    return GetExternalAccounts(clientId, RefreshTokenExchange(encryptedAccessKey), vendorID, vendorName, provider, false);
                }
                else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return accounts;

                dynamic objContent = JsonConvert.DeserializeObject(response.Content);
                var jsonAccounts = objContent["results"];
                foreach (var objAccount in jsonAccounts)
                {
                    string id = objAccount["account_id"];
                    string name = objAccount["display_name"];
                    ExternalAccount account = new ExternalAccount(id, name, vendorID, vendorName, provider, _datafeedDataService.IsExternalAccountMapped(clientId, id, vendorID, out string mappedAccount), mappedAccount);
                    accounts.Add(account);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return accounts;
        }

        public List<Transaction> GetAccountTransactions(string externalAccountID, string encryptedAccessKey, out decimal accountBalance, out decimal availableBalance, DateTime? dateFrom = null, DateTime? dateTo = null, bool refreshToken = true)
        {
            List<Transaction> transactions = new List<Transaction>();
            accountBalance = GetAccountBalance(externalAccountID, ref encryptedAccessKey, out availableBalance);
            try
            {
                string accesskey = SecurityService.DecryptTripleDES(encryptedAccessKey);
                var client = new RestClient($"{_ApiUrl}/data/v1/accounts/{externalAccountID}/transactions");
                var request = new RestRequest(Method.GET);
                request.AddParameter("from", DateTime.MinValue.ToString("yyyy-MM-ddTH:mm:ss"));
                request.AddParameter("to", DateTime.UtcNow.ToString("yyyy-MM-ddTH:mm:ss"));
                request.AddHeader("Authorization", $"Bearer {accesskey}");
                IRestResponse response = client.Execute(request);

                if (response.Content.Length > 0 && response.StatusCode == System.Net.HttpStatusCode.Unauthorized && refreshToken)
                {
                    return GetAccountTransactions(externalAccountID, RefreshTokenExchange(encryptedAccessKey), out accountBalance, out availableBalance, dateFrom = null, dateTo = null, false);
                }
                else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return transactions;


                dynamic objContent = JsonConvert.DeserializeObject(response.Content);
                if (objContent["results"] != null)
                    DeserialiseTransactions(objContent["results"], externalAccountID, ref transactions);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            transactions.AddRange(GetAccountPendingTransactions(externalAccountID, encryptedAccessKey, dateFrom, dateTo));

            return transactions;
        }

        private List<Transaction> GetAccountPendingTransactions(string externalAccountID, string encryptedAccessKey, DateTime? dateFrom = null, DateTime? dateTo = null, bool refreshToken = true)
        {
            List<Transaction> transactions = new List<Transaction>();
            try
            {
                string accesskey = SecurityService.DecryptTripleDES(encryptedAccessKey);
                var client = new RestClient($"{_ApiUrl}/data/v1/accounts/{externalAccountID}/transactions/pending");
                var request = new RestRequest(Method.GET);
                request.AddParameter("from", DateTime.MinValue.ToString("yyyy-MM-ddTH:mm:ss"));
                request.AddParameter("to", DateTime.UtcNow.ToString("yyyy-MM-ddTH:mm:ss"));
                request.AddHeader("Authorization", $"Bearer {accesskey}");
                IRestResponse response = client.Execute(request);

                if (response.Content.Length > 0 && response.StatusCode == System.Net.HttpStatusCode.Unauthorized && refreshToken)
                {
                    return GetAccountPendingTransactions(externalAccountID, RefreshTokenExchange(encryptedAccessKey), dateFrom = null, dateTo = null, false);
                }
                else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return transactions;


                dynamic objContent = JsonConvert.DeserializeObject(response.Content);
                if (objContent["results"] != null)
                    DeserialiseTransactions(objContent["results"], externalAccountID, ref transactions);

                transactions.ForEach(t => t.Status = Status.PENDING);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return transactions;
        }

        private void DeserialiseTransactions(dynamic transactionsNode, string accountId, ref List<Transaction> transactions)
        {
            if (transactions == null)
                transactions = new List<Transaction>();

            foreach (var transaction in transactionsNode)
            {
                try
                {
                    string transactionID = transaction["transaction_id"];
                    decimal.TryParse(transaction["amount"].ToString() ?? "0", out decimal amount);
                    DateTime.TryParse(transaction["timestamp"].ToString(), out DateTime date);
                    string category = ((string)transaction["transaction_category"]).ToTitleCase();
                    string type = category;
                    if ((transaction["transaction_classification"]).Count > 0 && !string.IsNullOrEmpty((transaction["transaction_classification"])[0].ToString()))
                        category = (transaction["transaction_classification"])[0].ToString();
                    string vendor = ((string)transaction["merchant_name"]).ToTitleCase();
                    if (string.IsNullOrEmpty(vendor))
                        vendor = ((string)transaction.meta?.provider_merchant_name ?? "").ToTitleCase();
                    string merchant = ((string)transaction["description"]).ToTitleCase();
					Transaction t = new Transaction(transactionID, date, accountId, category, amount, vendor, merchant, type);
					t.Owner = nameof(TrueLayerAPI);
                    transactions.Add(t);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private decimal GetAccountBalance(string externalAccountID, ref string encryptedAccessKey, out decimal availableBalance, bool refreshToken = true)
        {
            availableBalance = 0;
            try
            {
                string accesskey = SecurityService.DecryptTripleDES(encryptedAccessKey);
                var client = new RestClient($"{_ApiUrl}/data/v1/accounts/{externalAccountID}/balance");
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", $"Bearer {accesskey}");
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    dynamic objContent = JsonConvert.DeserializeObject(response.Content);
                    decimal.TryParse((string)objContent.results[0].current, out decimal amount);
                    decimal.TryParse((string)objContent.results[0].available, out availableBalance);
                    return amount;
                }
                else if (response.Content.Length > 0 && response.StatusCode == System.Net.HttpStatusCode.Unauthorized && refreshToken)
                {
                    encryptedAccessKey = RefreshTokenExchange(encryptedAccessKey);
                    return GetAccountBalance(externalAccountID, ref encryptedAccessKey, out availableBalance, false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return 0;
        }

        private string RefreshTokenExchange(string oldAccessKey)
        {
            try
            {
                string encryptedRefreshToken = _datafeedDataService.GetRefreshTokenByAccessKey(oldAccessKey);
                string refreshToken = SecurityService.DecryptTripleDES(encryptedRefreshToken);

                var client = new RestClient($"{_AuthUrl}/connect/token");
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("undefined", $"grant_type=refresh_token&client_id={_ClientId}&client_secret={_Secret}&refresh_token={refreshToken}", ParameterType.RequestBody);

                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    dynamic objContent = JsonConvert.DeserializeObject(response.Content);
                    string accessKey = objContent?.access_token;
                    string newRefreshToken = objContent?.refresh_token;
                    if (!string.IsNullOrEmpty(accessKey))
                    {
                        _datafeedDataService.UpdateAccessKey(SecurityService.EncryptTripleDES(accessKey), SecurityService.EncryptTripleDES(newRefreshToken), oldAccessKey, DateTime.Now);
                        return SecurityService.EncryptTripleDES(accessKey);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return oldAccessKey;
        }
    }
}
