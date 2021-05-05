using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceAPICore;
using FinanceAPIData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinanceAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : Controller
	{
        private readonly AppSettings _appSettings;
		private AuthenticationProcessor _authenticationProcessor;
		public AuthController(IOptions<AppSettings> appSettings, AuthenticationProcessor authenticationProcessor)
		{
            _appSettings = appSettings.Value;
			_authenticationProcessor = authenticationProcessor;
        }

		[HttpPost("authenticate")]
		public IActionResult Authenticate([FromBody][Required] AuthenticateRequest model)
		{
			Client client = _authenticationProcessor.AuthenticateClient(model.Username, model.Password);

			if (client == null)
				return Error.Generate("Username or password is incorrect", Error.ErrorType.InvalidCredentials);

			var token = generateJwtToken(client);

			return Json(token);
		}

		private string generateJwtToken(Client client)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes(_appSettings.JwtSecret);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new[] { new Claim("id", client.ID.ToString()) }),
				Expires = DateTime.UtcNow.AddHours(2),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}

		public class AuthenticateRequest
		{
			[Required]
			public string Username { get; set; }
			[Required]
			public string Password { get; set; }
		}
	}
}
