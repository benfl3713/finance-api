﻿using System;
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
	[Route("api/{clientId}")]
	[ApiController]
	public class EntityListController : Controller
	{
		private AccountProcessor _accountProcessor;
		private TransactionProcessor _transactionProcessor;
		public EntityListController(AccountProcessor accountProcessor, TransactionProcessor transactionProcessor)
		{
			_accountProcessor = accountProcessor;
			_transactionProcessor = transactionProcessor;
		}

		[HttpGet("accounts")]
		public IActionResult GetAccounts([FromRoute(Name = "clientId")] string clientId)
		{
			return Json(_accountProcessor.GetAccounts(clientId));
		}

		[HttpGet("transactions")]
		public IActionResult GetTransactions([FromRoute(Name = "clientId")] string clientId)
		{
			return Json(_transactionProcessor.GetTransactions(clientId));
		}
	}
}
