using System;
using System.Collections.Generic;
using FinanceAPICore;
using FinanceAPICore.DataService;
using FinanceAPICore.Tasks;
using FinanceAPICore.Wealth;
using FinanceAPIData.Datafeeds.WealthAPIs;
using FinanceAPIData.Wealth;
using Microsoft.Extensions.Options;

namespace FinanceAPIData.Tasks
{
    public class WealthRefreshTask : BaseTask
    {
        private readonly DatafeedProcessor _datafeedProcessor;
        private readonly IClientDataService _clientDataService;
        private readonly AssetRepository _assetRepository;
        private readonly TradeRepository _tradeRepository;
        private readonly IDatafeedDataService _datafeedDataService;
        private readonly IOptions<AppSettings> _appSettings;

        public WealthRefreshTask(IOptions<TaskSettings> settings, IOptions<AppSettings> appSettings, DatafeedProcessor datafeedProcessor, IClientDataService clientDataService, AssetRepository assetRepository, IDatafeedDataService datafeedDataService, TradeRepository tradeRepository) : base(settings)
        {
            _datafeedProcessor = datafeedProcessor;
            _clientDataService = clientDataService;
            _assetRepository = assetRepository;
            _datafeedDataService = datafeedDataService;
            _appSettings = appSettings;
            _tradeRepository = tradeRepository;
        }

        public override void Execute(Task task)
        {
            foreach (Client client in _clientDataService.GetAllClients())
            {
                if (task != null && !string.IsNullOrEmpty(task.ClientID) && task.ClientID != client.ID)
                    continue;
                
                List<Datafeed> datafeeds = _datafeedProcessor.GetDatafeeds(client.ID);
                foreach (Datafeed datafeed in datafeeds)
                {
                    if (!datafeedApis.ContainsKey(datafeed.Provider))
                        continue;

                    IWealthApi api = ResolveApiType(datafeed.Provider);
                    if (api == null)
                        continue;

                    var assets = api.GetAssets(client.ID).Result;

                    foreach (Asset asset in assets)
                    {
                        _assetRepository.ImportAsset(asset);
                        List<Trade> trades = api.GetTradesByAsset(client.ID, asset.Id).Result;
                        trades.ForEach(t => _tradeRepository.ImportTrade(t));
                    }
                }
            }
        }

        public IWealthApi ResolveApiType(string datafeedId)
        {
            if (!datafeedApis.ContainsKey(datafeedId))
                return null;

            Type datafeedType = datafeedApis[datafeedId];
            if (datafeedType == null)
                return null;

            //IOptions<AppSettings> appSettings, IDatafeedDataService datafeedDataService, AssetRepository assetRepository, TradeRepository tradeRepository
            return (IWealthApi)Activator.CreateInstance(datafeedType, _appSettings, _datafeedDataService, _assetRepository, _tradeRepository);
        }

        private static Dictionary<string, Type> datafeedApis = new Dictionary<string, Type>
        {
            { "COINBASE", typeof(CoinbaseApi) }
        };
    }
}