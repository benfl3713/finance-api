using System.ComponentModel.DataAnnotations;
using FinanceAPI.Attributes;
using FinanceAPICore;
using FinanceAPIData;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace FinanceAPI.Controllers
{
    [Route("api/goal")]
    [ApiController]
    [Authorize]
    public class GoalController : Controller
    {
        private GoalProcessor _goalProcessor;
        public GoalController(GoalProcessor goalProcessor)
        {
            _goalProcessor = goalProcessor;
        }

        [HttpGet]
        public IActionResult GetGoals()
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            return Json(_goalProcessor.GetGoals(clientId));
        }
        
        [HttpPost]
        public IActionResult InsertGoal([FromBody] JObject jGoal)
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            Goal goal = Goal.CreateFromJson(jGoal, clientId);
            if (!string.IsNullOrEmpty(goal.Id))
                return BadRequest("Goal Id should be null");

            return Json(_goalProcessor.InsertGoal(goal));
        }

        [HttpPut]
        public IActionResult UpdateGoal([FromBody] JObject jGoal)
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            Goal goal = Goal.CreateFromJson(jGoal, clientId);
            if (string.IsNullOrEmpty(goal.Id))
                return BadRequest("Goal Id should have a value");

            return Json(_goalProcessor.UpdateGoal(goal));
        }

        [HttpGet("{goalId}")]
        public IActionResult GetTransactionById([FromRoute(Name = "goalId")] [Required] string goalId)
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            Goal goal = _goalProcessor.GetGoalById(goalId, clientId);
            if (goal == null)
                return BadRequest("Could not find Goal");
            return Json(goal);
        }

        [HttpDelete("{goalId}")]
        public IActionResult DeleteTransaction([FromRoute(Name = "goalId")] [Required] string goalId)
        {
            string clientId = Request.HttpContext.Items["ClientId"]?.ToString();
            if (_goalProcessor.DeleteGoal(goalId, clientId))
                return Json("Goal Deleted");
            return Json(BadRequest("Failed to delete Goal"));
        }
    }
}