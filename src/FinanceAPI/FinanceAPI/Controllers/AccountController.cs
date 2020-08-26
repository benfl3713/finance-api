using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using FinanceAPICore;
using FinanceAPIData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FinanceAPI.Controllers
{
	[Route("api/{clientId}/account")]
	[ApiController]
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
			Account account = Account.CreateFromJson(jsonAccount);
			string accountId = _accountProcessor.InsertAccount(account);
			if (accountId != null)
				return Ok(accountId);
			return BadRequest();
		}

		[HttpPut]
		public IActionResult UpdateAccount([FromBody] JObject jsonAccount)
		{
			Account account = Account.CreateFromJson(jsonAccount);
			if (string.IsNullOrEmpty(account.ID))
				return BadRequest("Account ID is required");

			if (_accountProcessor.UpdateAccount(account))
				return Ok("Account Updated");
			return BadRequest();
		}

		[HttpGet("{accountId}")]
		public IActionResult GetAccountById([FromRoute(Name = "accountId")][Required] string accountId)
		{
			Account account = _accountProcessor.GetAccountById(accountId);
			if (account == null)
				return BadRequest("Could not find account");
			return Json(account);
		}

		[HttpDelete("{accountId}")]
		public IActionResult DeleteAccount([FromRoute(Name = "accountId")][Required] string accountId)
		{
			if (_accountProcessor.DeleteAccount(accountId))
				return Ok("Account Deleted");
			return BadRequest("Failed to delete Account");
		}
	}
}
