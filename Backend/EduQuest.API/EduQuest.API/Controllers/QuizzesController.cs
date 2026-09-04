using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EduQuest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly ApplicationDBContext dBContext;

        public QuizzesController(ApplicationDBContext dBContext)
        {
            this.dBContext = dBContext;
        }

        [HttpGet]
        public IActionResult GetAllQuizzes()
        {
            return Ok(dBContext.Quizzes.ToList());
        }

        [HttpPost]
        public IActionResult AddQuiz(AddQuizDto addQuizDto)
        {
            var QuizEntity = new Quiz()
            {
                QuizTitle = addQuizDto.QuizTitle,
                Difficulty = addQuizDto.Difficulty,
                TimeLimit = addQuizDto.TimeLimit,
                IsPublished = addQuizDto.IsPublished

            };
            // still needs some fixing
            dBContext.Quizzes.Add(QuizEntity);
            dBContext.SaveChanges();

            return Ok(QuizEntity);
        }
    }
}
