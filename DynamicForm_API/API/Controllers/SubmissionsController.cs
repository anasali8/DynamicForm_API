using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Submissions;
using DynamicForm_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DynamicForm_API.API.Controllers
{
    [ApiController]
    public class SubmissionsController : ControllerBase
    {
        private readonly ISubmissionService _submissionService;

        public SubmissionsController(ISubmissionService submissionService)
        {
            _submissionService = submissionService;
        }

        [HttpPost("/api/forms/{formCode}/submit")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<FormSubmissionCreatedResponseDto>>> SubmitForm(
            string formCode,
            [FromBody] SubmitFormVersionRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _submissionService.SubmitFormAsync(formCode, request, cancellationToken);
                return Ok(Success(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Problem(title: "Not found", detail: ex.Message, statusCode: StatusCodes.Status404NotFound));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(Problem(title: "Invalid request", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(Problem(title: "Operation failed", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest));
            }
        }

        [HttpGet("/api/forms/{formCode}/submissions")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<PagedResponseDto<FormSubmissionListItemResponseDto>>>> GetFormSubmissions(
            string formCode,
            [FromQuery] PagedRequestDto request,
            [FromQuery] string? userId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _submissionService.GetFormSubmissionsAsync(formCode, request, userId, cancellationToken);
                return Ok(Success(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Problem(title: "Not found", detail: ex.Message, statusCode: StatusCodes.Status404NotFound));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(Problem(title: "Operation failed", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest));
            }
        }

        [HttpGet("/api/submissions/{submissionId:int}")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<FormSubmissionDetailResponseDto>>> GetSubmissionDetails(
            int submissionId,
            CancellationToken cancellationToken)
        {
            var result = await _submissionService.GetSubmissionDetailsByIdAsync(submissionId, cancellationToken);
            if (result is null)
            {
                return NotFound(Problem(title: "Not found", detail: $"Submission '{submissionId}' was not found.", statusCode: StatusCodes.Status404NotFound));
            }

            return Ok(Success(result));
        }

        private static ApiSuccessEnvelopeDto<T> Success<T>(T data) => new()
        {
            Success = true,
            Data = data,
            Message = null
        };
    }
}
