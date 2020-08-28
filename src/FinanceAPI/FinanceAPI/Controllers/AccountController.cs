using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FinanceAPI.Attributes;
using FinanceAPICore;
using FinanceAPIData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FinanceAPI.Controllers
{
	[Route("api/account")]
	[ApiController]
	[Authorize]
	public class AccountController : Controller
	{
		private AccountProcessor _accountProcessor;
		public AccountController(AccountProcessor accountProcessor)
		{
			_accountProcessor = accountProcessor;
		}
		[HttpPost]
		public IActionResult InsertAccount([FromBody] JObject jsonAccount)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			Account account = Account.CreateFromJson(jsonAccount, clientId);
			string accountId = _accountProcessor.InsertAccount(account);
			if (accountId != null)
				return Ok(accountId);
			return BadRequest();
		}

		[HttpPut]
		public IActionResult UpdateAccount([FromBody] JObject jsonAccount)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			Account account = Account.CreateFromJson(jsonAccount, clientId);
			if (string.IsNullOrEmpty(account.ID))
				return BadRequest("Account ID is required");

			if (_accountProcessor.UpdateAccount(account))
				return Ok("Account Updated");
			return BadRequest();
		}

		[HttpGet("{accountId}")]
		public IActionResult GetAccountById([FromRoute(Name = "accountId")][Required] string accountId)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			Account account = _accountProcessor.GetAccountById(accountId, clientId);
			if (account == null)
				return BadRequest("Could not find account");
			return Json(account);
		}

		[HttpDelete("{accountId}")]
		public IActionResult DeleteAccount([FromRoute(Name = "accountId")][Required] string accountId)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			if (_accountProcessor.DeleteAccount(accountId, clientId))
				return Ok("Account Deleted");
			return BadRequest("Failed to delete Account");
		}
	}
}
