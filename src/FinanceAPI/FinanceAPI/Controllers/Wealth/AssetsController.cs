using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FinanceAPI.Attributes;
using FinanceAPICore.Wealth;
using FinanceAPIData.Wealth;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAPI.Controllers.Wealth
{
    [ApiController]
    [Route("api/wealth/assets")]
    [Authorize]
	[Produces("application/json")]
    public class AssetsController : Controller
    {
        private readonly AssetRepository _assetRepository;

        public AssetsController(AssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
        }
        
        [HttpGet]
        public List<Asset> GetAssets()
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return _assetRepository.GetAssets(clientId);
        }

        [HttpGet("{assetId}")]
        public Asset GetAssetById([FromRoute(Name = "assetId")] [Required] string assetId)
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return _assetRepository.GetAssetById(assetId, clientId);
        }

        [HttpPost]
        public bool InsertAsset([Required] Asset asset)
        {
            asset.ClientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return _assetRepository.InsertAsset(asset);
        }

        [HttpPut]
        [HttpPut("{assetId}")]
        public bool UpdateAsset([Required] Asset asset)
        {
            asset.ClientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return _assetRepository.UpdateAsset(asset);
        }

        [HttpDelete("{assetId}")]
        public bool DeleteAsset([FromRoute(Name = "assetId")] [Required] string assetId)
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return _assetRepository.DeleteAsset(assetId, clientId);
        }
    }
}