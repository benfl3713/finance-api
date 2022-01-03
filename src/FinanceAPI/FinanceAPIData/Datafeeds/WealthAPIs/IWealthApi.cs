using System.Collections.Generic;
using System.Threading.Tasks;
using FinanceAPICore.Wealth;

public interface IWealthApi
{
    Task<List<Asset>> GetAssets(string clientId);
    Task GetTradesByAsset(string clientId, string assetId);
} 