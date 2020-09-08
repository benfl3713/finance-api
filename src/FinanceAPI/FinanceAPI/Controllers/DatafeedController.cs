using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FinanceAPI.Attributes;
using FinanceAPICore;
using FinanceAPIData;
using FinanceAPIData.Datafeeds;
using FinanceAPIData.Datafeeds.APIs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace FinanceAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class DatafeedController : Controller
	{
		private DatafeedProcessor _datafeedProcessor;
		private AppSettings _appSettings;
		public DatafeedController(DatafeedProcessor datafeedProcessor, IOptions<AppSettings> appSettings)
		{
			_datafeedProcessor = datafeedProcessor;
			_appSettings = appSettings.Value;
		}

		[HttpGet("[action]")]
		public IActionResult GetExternalAccounts()
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			List<Datafeed> datafeeds = _datafeedProcessor.GetDatafeeds(clientId);
			List<ExternalAccount> accounts = new List<ExternalAccount>();
			foreach (var datafeed in datafeeds)
			{
				IDatafeedAPI datafeedApi = DatafeedManager.ResolveApiType(datafeed.Provider);
				if(datafeedApi is TrueLayerAPI trueLayerAPI)
				{
					trueLayerAPI._ClientId = _appSettings.TrueLayer_ClientID;
					trueLayerAPI._Secret = _appSettings.TrueLayer_ClientSecret;
				}
				accounts.AddRange(datafeedApi.GetExternalAccounts(clientId, datafeed.AccessKey, datafeed.VendorID, datafeed.VendorName, datafeed.Provider));
			}

			return Json(accounts);
		}

		[HttpPost("[action]")]
		public IActionResult AddExternalAccountMapping([FromBody] JObject jExternalAccount)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			var datafeed = jExternalAccount["provider"].ToString();
			var vendorID = jExternalAccount["vendorID"].ToString();
			var accountID = jExternalAccount["accountID"].ToString();
			var externalAccountID = jExternalAccount["externalAccountID"].ToString();

			return _datafeedProcessor.AddExternalAccountMapping(clientId, datafeed, vendorID, accountID, externalAccountID) 
				? Json(null) as IActionResult 
				: BadRequest();
		}

		[HttpDelete("[action]")]
		public IActionResult RemoveExternalAccountMapping([Required] string accountId)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();

			return _datafeedProcessor.RemoveExternalAccountMapping(clientId, accountId)
				? Json(null) as IActionResult
				: BadRequest();
		}
	}
}
