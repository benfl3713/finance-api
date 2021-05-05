using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceAPICore.Attributes;
using FinanceAPICore.DataService;
using FinanceAPICore.Middleware;
using FinanceAPIData.Datafeeds.APIs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Serilog.Events;

namespace FinanceAPICore.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DatafeedAuthController : Controller
	{
        private JwtMiddleware _jwtMiddleware;
        private AppSettings _appSettings;
        private IDatafeedDataService _datafeedDataService;
		public DatafeedAuthController(JwtMiddleware jwtMiddleware, IOptions<AppSettings> appSettings, IDatafeedDataService datafeedDataService)
		{
            _jwtMiddleware = jwtMiddleware;
            _appSettings = appSettings.Value;
            _datafeedDataService = datafeedDataService;
        }

        [Authorize]
        [HttpGet("[action]")]
        public IActionResult GetTrueLayerClientId()
		{
            return Json(_appSettings.TrueLayer_ClientID);
		}

        [HttpPost("[action]")]
        public string TrueLayerAuthentication()
        {
            try
            {
                var form = Request.Form;
                form.TryGetValue("code", out StringValues codeStrV);
                form.TryGetValue("state", out StringValues stateStrV);
                form.TryGetValue("error", out StringValues errors);

                //Validate Request data
                if (string.IsNullOrEmpty(codeStrV) || codeStrV.Count != 1 || string.IsNullOrEmpty(stateStrV) || stateStrV.Count != 1 || !string.IsNullOrEmpty(errors))
                {
                    Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                    return "Something went wrong";
                }

                string sessionID = stateStrV[0];
                string code = codeStrV[0];
                string existingId = null;

                if (sessionID.Contains("|"))
                {
                    string[] stateParts = sessionID.Split('|');
                    sessionID = stateParts[0];
                    existingId = stateParts[1];
                }

                string scheme = Request.Scheme;
                if (!string.IsNullOrEmpty(Request.Headers["X-Forwarded-Proto"]))
                    scheme = Request.Headers["X-Forwarded-Proto"];

                var location = new Uri($"{scheme}://{Request.Host}{Request.Path}{Request.QueryString}");

                var clientId = _jwtMiddleware.GetClientIdFromToken(sessionID);
                if (clientId == null)
                    return "Invalid User";
                

                TrueLayerAPI trueLayerAPI = new TrueLayerAPI(_datafeedDataService, _appSettings.TrueLayer_ClientID, _appSettings.TrueLayer_ClientSecret, _appSettings.TrueLayer_Mode);
                return trueLayerAPI.RegisterNewClient(code, clientId, location.AbsoluteUri, existingId) ? "Datafeed has been Added. \nPlease Refresh finance manager" : "Something went wrong";

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "Something went wrong. The error has been logged in the console";
            }
        }
    }
}
