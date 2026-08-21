using EduQuest.API.DTOs.Learners;
using Microsoft.AspNetCore.Mvc;

namespace EduQuest.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class LearnersController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetLearners()
        {
            return Ok();
        }

        [HttpGet("{id}")]
        public IActionResult GetLearner(int id)
        {
            return Ok();
        }

        [HttpPost]
        public IActionResult CreateLearner(CreateLearnerDto dto)
        {
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateLearner(int id, UpdateLearnerDto dto)
        {
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteLearner(int id)
        {
            return Ok();
        }
    }

}
