using Microsoft.AspNetCore.Mvc;

namespace EduQuest.API.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class ActivityLogsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetActivityLogs()
        {
            return Ok();
        }

        [HttpGet("{id}")]
        public IActionResult GetActivityLog(int id)
        {
            return Ok();
        }
    }

}
