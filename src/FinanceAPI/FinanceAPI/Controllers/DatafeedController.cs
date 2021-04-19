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
		private TaskProcessor _taskProcessor;
		private AppSettings _appSettings;
		public DatafeedController(DatafeedProcessor datafeedProcessor, IOptions<AppSettings> appSettings, TaskProcessor taskProcessor)
		{
			_datafeedProcessor = datafeedProcessor;
			_appSettings = appSettings.Value;
			_taskProcessor = taskProcessor;
		}

		[HttpGet]
		public IActionResult GetDatafeeds(string datafeedType = null)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			return Json(_datafeedProcessor.GetDatafeeds(clientId, datafeedType));
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
					trueLayerAPI.SetMode(_appSettings.TrueLayer_Mode);
					trueLayerAPI._datafeedDataService = new FinanceAPIMongoDataService.DataService.DatafeedDataService(_appSettings.MongoDB_ConnectionString);
				}
				accounts.AddRange(datafeedApi.GetExternalAccounts(clientId, datafeed.AccessKey, datafeed.VendorID, datafeed.VendorName, datafeed.Provider));
			}

			List<ExternalAccount> mappedAccounts = _datafeedProcessor.GetExternalAccounts(clientId);
			foreach (ExternalAccount account in mappedAccounts)
			{
				if (!accounts.Any(a => a.AccountID == account.AccountID))
					accounts.Add(account);
			}

			return Json(accounts);
		}

		[HttpGet("[action]")]
		public IActionResult GetMappedExternalAccounts(string accountId = null)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			return Json(_datafeedProcessor.GetExternalAccounts(clientId, accountId));
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
		public IActionResult RemoveExternalAccountMapping([Required] string accountId, [Required] string externalAccountId)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();

			return _datafeedProcessor.RemoveExternalAccountMapping(clientId, accountId, externalAccountId)
				? Json(null) as IActionResult
				: BadRequest();
		}

		[HttpDelete("[action]")]
		public IActionResult DeleteDatafeed([Required] string provider, [Required] string vendorId)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();

			return _datafeedProcessor.DeleteClientDatafeed(clientId, provider, vendorId)
				? Json(null) as IActionResult
				: BadRequest();
		}

		[HttpGet("[action]")]
		public IActionResult RefreshAccount([Required] string accountId)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();

			return _taskProcessor.RefreshAccount(clientId, accountId)
				? Json("Account is refreshing in the background") as IActionResult
				: BadRequest();
		}
	}
}
