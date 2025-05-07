using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("for-admin")]
        [Authorize(Policy =Constants.Policies.Admin)]
        public IEnumerable<string> GetForAdmin()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("for-user")]
        [Authorize(Policy = Constants.Policies.User)]
        public IEnumerable<string> GetForUser()
        {
            return new string[] { "value1", "value2" };
        }
    }
}
