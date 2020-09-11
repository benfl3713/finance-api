using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceAPI.Attributes;
using FinanceAPI.Middleware;
using FinanceAPIData.Datafeeds.APIs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace FinanceAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DatafeedAuthController : Controller
	{
        private JwtMiddleware _jwtMiddleware;
        private AppSettings _appSettings;
		public DatafeedAuthController(JwtMiddleware jwtMiddleware, IOptions<AppSettings> appSettings)
		{
            _jwtMiddleware = jwtMiddleware;
            _appSettings = appSettings.Value;
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
                var location = new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");

                var clientId = _jwtMiddleware.GetClientIdFromToken(sessionID);
                if (clientId == null)
                    return "Invalid User";

                TrueLayerAPI trueLayerAPI = new TrueLayerAPI(_appSettings.MongoDB_ConnectionString, _appSettings.TrueLayer_ClientID, _appSettings.TrueLayer_ClientSecret, _appSettings.TrueLayer_Mode);
                return trueLayerAPI.RegisterNewClient(code, clientId, location.AbsoluteUri) ? "Datafeed has been Added. \nPlease Refresh finance manager" : "Something went wrong";

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "Something went wrong. The error has been logged in the console";
            }
        }
    }
}
