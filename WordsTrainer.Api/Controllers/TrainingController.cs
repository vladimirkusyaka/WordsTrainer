using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WordsTrainer.Api.Extensions;
using WordsTrainer.Api.Services;
using WordsTrainer.Contracts.Common;
using WordsTrainer.Contracts.Training;

namespace WordsTrainer.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/training")]
    public class TrainingController : ControllerBase
    {
        private readonly TrainingService _trainingService;

        public TrainingController(TrainingService trainingService)
        {
            _trainingService = trainingService;
        }

        [HttpGet("next")]
        public async Task<ActionResult<TrainingNextResponse>> GetNext()
        {
            var userId = User.GetUserId();

            var result = await _trainingService.GetNextAsync(userId);

            return Ok(result);
        }

        [HttpPost("answer")]
        public async Task<ActionResult<SubmitTrainingAnswerResponse>> SubmitAnswer(
            SubmitTrainingAnswerRequest request)
        {
            var userId = User.GetUserId();

            var result = await _trainingService.SubmitAnswerAsync(userId, request);

            if (result == null)
                return BadRequest(Error(
                    "training.answer.invalid",
                    "Invalid training answer."));

            return Ok(result);
        }

        [HttpGet("explanation/attempt/{attemptId:guid}")]
        public async Task<ActionResult<TrainingExplanationResponse>> GetExplanationByAttempt(Guid attemptId)
        {
            var userId = User.GetUserId();

            var result = await _trainingService.GetExplanationByAttemptAsync(
                userId,
                attemptId);

            if (result == null)
                return NotFound(Error(
                    "explanation.not.found",
                    "Explanation not found."));

            return Ok(result);
        }

        [HttpGet("stats")]
        public async Task<ActionResult<TrainingStatsResponse>> GetStats()
        {
            var userId = User.GetUserId();

            var result = await _trainingService.GetStatsAsync(userId);

            return Ok(result);
        }

        [HttpPost("session/start")]
        public async Task<ActionResult<TrainingSessionResponse>> StartSession()
        {
            var userId = User.GetUserId();

            var result = await _trainingService.StartSessionAsync(userId);

            return Ok(result);
        }

        [HttpGet("session/current")]
        public async Task<ActionResult<TrainingSessionResponse>> GetCurrentSession()
        {
            var userId = User.GetUserId();

            var result = await _trainingService.GetCurrentSessionAsync(userId);

            if (result == null)
                return NotFound(Error(
                    "training.session.not.found",
                    "No active training session."));

            return Ok(result);
        }

        [HttpPost("session/finish")]
        public async Task<ActionResult<TrainingSessionResponse>> FinishSession()
        {
            var userId = User.GetUserId();

            var result = await _trainingService.FinishSessionAsync(userId);

            if (result == null)
                return NotFound(Error(
                    "training.session.not.found",
                    "No active training session."));

            return Ok(result);
        }

        private static ApiErrorResponse Error(string code, string message)
        {
            return new ApiErrorResponse
            {
                Code = code,
                Message = message
            };
        }
    }
}
