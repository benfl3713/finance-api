using System.Collections.Generic;
using FinanceAPICore.Wealth;

namespace FinanceAPICore.DataService.Wealth
{
    public interface IAssetDataService
    {
        public bool InsertAsset(Asset asset);
        public bool UpdateAsset(Asset asset);
        public bool DeleteAsset(string id, string clientId);
        public List<Asset> GetAssets(string clientId);
        public Asset GetAssetById(string assetId, string clientId);
    }
}