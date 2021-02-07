using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace FinanceAPI
{
    public static class Error
    {
        public static IActionResult Generate(string message, ErrorType errorType, Dictionary<string, object> extraDetails = null)
        {
            dynamic response = new
            {
                message,
                errorType = Enum.GetName(typeof(ErrorType), errorType),
                method = new StackTrace().GetFrame(1)?.GetMethod()?.Name
            };

            if (extraDetails != null)
            {
                foreach (KeyValuePair<string,object> valuePair in extraDetails)
                {
                    response[valuePair.Key] = valuePair.Value;
                }
            }

            var action = new BadRequestObjectResult(response);
            action.ContentTypes = new MediaTypeCollection();
            action.ContentTypes.Add("application/json");

            return action;
        }

        public enum ErrorType
        {
            General,
            NotExist,
            CreateFailure,
            UpdateFailure,
            DeleteFailure,
            MissingParameters,
            InvalidCredentials
        }
    }
}