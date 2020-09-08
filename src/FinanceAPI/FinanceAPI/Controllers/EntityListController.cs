using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceAPI.Attributes;
using FinanceAPICore;
using FinanceAPIData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceAPI.Controllers
{
	[Authorize]
	[Route("api")]
	[ApiController]
	public class EntityListController : Controller
	{
		private AccountProcessor _accountProcessor;
		private TransactionProcessor _transactionProcessor;
		private DatafeedProcessor _datafeedProcessor;
		public EntityListController(AccountProcessor accountProcessor, TransactionProcessor transactionProcessor, DatafeedProcessor datafeedProcessor)
		{
			_accountProcessor = accountProcessor;
			_transactionProcessor = transactionProcessor;
			_datafeedProcessor = datafeedProcessor;
		}

		[HttpGet("accounts")]
		public IActionResult GetAccounts()
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			return Json(_accountProcessor.GetAccounts(clientId));
		}

		[HttpGet("transactions")]
		public IActionResult GetTransactions(string accountId = null)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			return Json(_transactionProcessor.GetTransactions(clientId, accountId));
		}

		[HttpGet("datafeeds")]
		public IActionResult GetDatafeeds(string datafeedType = null)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			return Json(_datafeedProcessor.GetDatafeeds(clientId, datafeedType));
		}
	}
}
