using System;
using System.Collections.Generic;
using FinanceAPICore.DataService.Wealth;
using FinanceAPICore.Wealth;

namespace FinanceAPIData.Wealth
{
    public class AssetRepository
    {
        private readonly IAssetDataService _assetDataService; 
        public AssetRepository(IAssetDataService assetDataService)
        {
            _assetDataService = assetDataService;
        }
        
        public List<Asset> GetAssets(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentException("Unknown Client");

            return _assetDataService.GetAssets(clientId);
        }

        public bool InsertAsset(Asset asset)
        {
            Asset.Validate(asset);
            return _assetDataService.InsertAsset(asset);
        }

        public bool UpdateAsset(Asset asset)
        {
            Asset.Validate(asset);
            if (asset.Id == null)
                throw new ArgumentException("Asset Id is required", nameof(asset.Id));

            return _assetDataService.UpdateAsset(asset);
        }

        public Asset GetAssetById(string assetId, string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId), "clientId is required");

            if (string.IsNullOrEmpty(assetId))
                throw new ArgumentNullException(nameof(assetId), "AssetId is required");

            return _assetDataService.GetAssetById(assetId, clientId);
        }

        public bool DeleteAsset(string assetId, string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                throw new ArgumentNullException(nameof(clientId), "clientId is required");

            if (string.IsNullOrEmpty(assetId))
                throw new ArgumentNullException(nameof(assetId), "AssetId is required");

            return _assetDataService.DeleteAsset(assetId, clientId);
        }

        public bool ImportAsset(Asset asset)
        {
            try
            {
                Asset existingAsset = GetAssetById(asset.Id, asset.ClientId);
                if (existingAsset != null)
                {
                    if (existingAsset.Owner == "User")
                        return false;

                    return UpdateAsset(asset);
                }
            }
            catch
            {
                // Means we could not find the asset
            }

            return InsertAsset(asset);
        }
    }
}