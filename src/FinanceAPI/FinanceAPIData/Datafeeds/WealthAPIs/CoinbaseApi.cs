using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coinbase;
using Coinbase.Models;
using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Utilities;
using FinanceAPICore.Wealth;
using FinanceAPIData.Tasks;
using FinanceAPIData.Wealth;
using Hangfire;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using CoinbaseAccount = Coinbase.Models.Account;
using Transaction = Coinbase.Models.Transaction;

namespace FinanceAPIData.Datafeeds.WealthAPIs
{
    public class CoinbaseApi : IWealthApi
    {
        private readonly AppSettings _appSettings;
        private readonly IDatafeedDataService _datafeedDataService;
        private readonly AssetRepository _assetRepository;
        private readonly TradeRepository _tradeRepository;
        private const string DATAFEED_NAME = "COINBASE";

        public CoinbaseApi(IOptions<AppSettings> appSettings, IDatafeedDataService datafeedDataService, AssetRepository assetRepository, TradeRepository tradeRepository)
        {
            _appSettings = appSettings.Value;
            _datafeedDataService = datafeedDataService;
            _assetRepository = assetRepository;
            _tradeRepository = tradeRepository;

            if (string.IsNullOrEmpty(_appSettings.CoinBase_ClientId) || string.IsNullOrEmpty(_appSettings.CoinBase_ClientSecret))
                throw new ApplicationException($"Coinbase api does not have required config. You need to have: [{nameof(_appSettings.CoinBase_ClientId)}, {nameof(_appSettings.CoinBase_ClientSecret)}]");
        }

        public async Task<List<Asset>> GetAssets(string clientId)
        {
            List<Asset> assets = new List<Asset>();
            CoinbaseClient client = GetClient(clientId);
            bool moreData = true;
            PagedResponse<CoinbaseAccount> accountPage = await client.Accounts.ListAccountsAsync();

            while (moreData)
            {
                if (accountPage.HasError())
                {
                    Console.WriteLine(accountPage.Errors);
                    throw new Exception(string.Join<Error>('|', accountPage.Errors));
                }

                assets.AddRange(ProcessAccountPage(accountPage, clientId));
                moreData = accountPage.HasNextPage();
                if (moreData)
                    accountPage = await client.GetNextPageAsync(accountPage);
            }

            return assets;
        }

        private List<Asset> ProcessAccountPage(PagedResponse<CoinbaseAccount> accountPage, string clientId)
        {
            List<Asset> assets = new List<Asset>();
            foreach (CoinbaseAccount coinbaseAccount in accountPage.Data)
            {
                if (coinbaseAccount?.CreatedAt == null)
                    continue;

                Asset asset = new Asset
                {
                    ClientId = clientId,
                    Id = coinbaseAccount.Id,
                    Name = coinbaseAccount.Name,
                    LastUpdated = DateTime.Now,
                    Type = coinbaseAccount.Type.Equals("fiat", StringComparison.OrdinalIgnoreCase) ? Asset.AssetTypes.Fiat : Asset.AssetTypes.Crypto,
                    Code = coinbaseAccount.Currency.Code,
                    Balance = coinbaseAccount.Balance.Amount,
                    Currency = coinbaseAccount.Balance.Currency,
                    Source = DATAFEED_NAME,
                    Owner = DATAFEED_NAME,
                    MarketIdentifiers = new Dictionary<string, string> { { "CurrencyName", coinbaseAccount.Currency.Name } }
                };

                assets.Add(asset);
            }

            return assets;
        }

        public async Task<List<Trade>> GetTradesByAsset(string clientId, string assetId)
        {
            List<Trade> trades = new List<Trade>();
            CoinbaseClient client = GetClient(clientId);

            bool moreData = true;
            PagedResponse<Transaction> transactionPage = await client.Transactions.ListTransactionsAsync(assetId);

            while (moreData)
            {
                if (transactionPage.HasError())
                {
                    Console.WriteLine(transactionPage.Errors);
                    throw new Exception(string.Join<Error>('|', transactionPage.Errors));
                }

                trades.AddRange(ProcessTradePage(transactionPage, clientId, assetId));
                moreData = transactionPage.HasNextPage();
                if (moreData)
                    transactionPage = await client.GetNextPageAsync(transactionPage);
            }

            return trades;
        }

        private List<Trade> ProcessTradePage(PagedResponse<Transaction> tradePage, string clientId, string assetId)
        {
            List<Trade> trades = new List<Trade>();
            foreach (Transaction coinbaseTrade in tradePage.Data)
            {
                if (coinbaseTrade?.CreatedAt == null)
                    continue;

                string description = coinbaseTrade.Description;

                if (string.IsNullOrEmpty(description) && coinbaseTrade.Details.ContainsKey("title"))
                    description = coinbaseTrade.Details["title"]?.ToString();

                Trade trade = new Trade
                {
                    Id = coinbaseTrade.Id,
                    Description = description,
                    Amount = coinbaseTrade.Amount.Amount,
                    Currency = coinbaseTrade.Amount.Currency,
                    Status = coinbaseTrade.Status,
                    TradeDateTime = coinbaseTrade.CreatedAt.Value.UtcDateTime,
                    AssetId = assetId,
                    ClientID = clientId,
                    Source = DATAFEED_NAME,
                    Owner = DATAFEED_NAME,
                    ExtraDetails = new Dictionary<string, string>
                    {
                        { "NativeAmount", JsonConvert.SerializeObject(coinbaseTrade.NativeAmount) }
                    }
                };

                trades.Add(trade);
            }

            return trades;
        }

        public async Task Authenticate(string clientId, string code, string redirectId)
        {
            OAuthResponse token = await OAuthHelper.GetAccessTokenAsync(code, _appSettings.CoinBase_ClientId, _appSettings.CoinBase_ClientSecret, redirectId);

            if (string.IsNullOrEmpty(token?.AccessToken) || string.IsNullOrEmpty(token?.RefreshToken))
                throw new ApplicationException("Could not authenticate with coinbase");

            Datafeed datafeed = new Datafeed(clientId, DATAFEED_NAME, DATAFEED_NAME, "Coinbase", DateTime.Now, SecurityService.EncryptTripleDES(token.AccessToken), SecurityService.EncryptTripleDES(token.RefreshToken));
            if (!_datafeedDataService.AddUpdateClientDatafeed(datafeed))
                throw new Exception("Could not save new datafeed connection");

            BackgroundJob.Enqueue<WealthRefreshTask>(r => r.Execute(new FinanceAPICore.Tasks.Task{ClientID = clientId}));
        }

        private CoinbaseClient GetClient(string clientId)
        {
            var datafeed = _datafeedDataService.GetDatafeeds(clientId).Find(d => d.Provider == DATAFEED_NAME);

            if (datafeed == null)
                throw new ArgumentException("Could not find cointbase datafeed for client");

            string accessToken = SecurityService.DecryptTripleDES(datafeed.AccessKey);
            string refreshToken = SecurityService.DecryptTripleDES(datafeed.RefreshKey);

            return new CoinbaseClient(new OAuthConfig
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }).WithAutomaticOAuthTokenRefresh(_appSettings.CoinBase_ClientId, _appSettings.CoinBase_ClientSecret, t => Task.Run(() => { SaveNewRefreshToken(t, accessToken); }));
        }

        private void SaveNewRefreshToken(OAuthResponse token, string encryptedOldAccessKey)
        {
            if (string.IsNullOrEmpty(token?.AccessToken) || string.IsNullOrEmpty(token?.RefreshToken))
                throw new ApplicationException("Could not authenticate with coinbase");

            _datafeedDataService.UpdateAccessKey(SecurityService.EncryptTripleDES(token.AccessToken), SecurityService.EncryptTripleDES(token.RefreshToken), encryptedOldAccessKey, DateTime.Now);
        }
    }
}