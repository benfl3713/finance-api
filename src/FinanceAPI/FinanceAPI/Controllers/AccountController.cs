using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FinanceAPI.Attributes;
using FinanceAPICore;
using FinanceAPIData;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FinanceAPI.Controllers
{
	[Route("api/account")]
	[ApiController]
	[Authorize]
	[Produces("application/json")]
	public class AccountController : Controller
	{
		private AccountProcessor _accountProcessor;
		public AccountController(AccountProcessor accountProcessor)
		{
			_accountProcessor = accountProcessor;
		}

		/// <summary>
		/// Gets a List of all the clients accounts
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public List<Account> GetAccounts()
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			return _accountProcessor.GetAccounts(clientId);
		}
		
		[HttpPost]
		public IActionResult InsertAccount([FromBody] JObject jsonAccount)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			Account account = Account.CreateFromJson(jsonAccount, clientId);
			string accountId = _accountProcessor.InsertAccount(account);
			if (accountId != null)
				return Json(accountId);
			return Error.Generate("Failed to Create Account", Error.ErrorType.CreateFailure);
		}

		[HttpPut("{accountId}")]
		public IActionResult UpdateAccount([FromBody] JObject jsonAccount)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			Account account = Account.CreateFromJson(jsonAccount, clientId);
			if (string.IsNullOrEmpty(account.ID))
				return Error.Generate("Account ID required", Error.ErrorType.MissingParameters);

			if (_accountProcessor.UpdateAccount(account))
				return Json("Account Updated");
			return Error.Generate("Failed to Update Account", Error.ErrorType.UpdateFailure);
		}

		[HttpGet("{accountId}")]
		public IActionResult GetAccountById([FromRoute(Name = "accountId")][Required] string accountId)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			Account account = _accountProcessor.GetAccountById(accountId, clientId);
			if (account == null)
				return Error.Generate("Could not find account", Error.ErrorType.NotExist);
			return Json(account);
		}

		[HttpDelete("{accountId}")]
		public IActionResult DeleteAccount([FromRoute(Name = "accountId")][Required] string accountId)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			if (_accountProcessor.DeleteAccount(accountId, clientId))
				return Json("Account Deleted");
			return Error.Generate("Failed to delete Account", Error.ErrorType.DeleteFailure);
		}

		[HttpGet("{accountId}/[action]")]
		public IActionResult GetSpentThisWeek([FromRoute(Name = "accountId")] [Required] string accountId)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			return Json(_accountProcessor.GetSpentThisWeek(accountId, clientId));
		}

		[HttpGet("{accountId}/[action]")]
		public IActionResult GetAccountSettings([FromRoute(Name = "accountId")] [Required]
			string accountId)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			return Json(_accountProcessor.GetAccountSettings(accountId, clientId));
		}

		[HttpPost("{accountId}/[action]")]

		public IActionResult SetAccountSettings([FromBody][Required] AccountSettings accountSettings)
		{
			string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
			return Json(_accountProcessor.SetAccountSettings(accountSettings, clientId));
		}
	}
}
