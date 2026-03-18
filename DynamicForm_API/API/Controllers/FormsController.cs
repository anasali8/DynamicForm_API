using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Forms;
using DynamicForm_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DynamicForm_API.API.Controllers
{
    [ApiController]
    [Route("api/forms")]
    public class FormsController : ControllerBase
    {
        private readonly IFormService _formService;

        public FormsController(IFormService formService)
        {
            _formService = formService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<PagedResponseDto<FormSummaryResponseDto>>>> GetAvailableForms(
            [FromQuery] PagedRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _formService.GetFormsAsync(request, cancellationToken);
            return Ok(Success(result));
        }

        [HttpGet("{formCode}/current")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<FormWithCurrentPublishedVersionResponseDto>>> GetCurrentPublishedForm(
            string formCode,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _formService.GetCurrentPublishedFormAsync(formCode, cancellationToken);
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

        private static ApiSuccessEnvelopeDto<T> Success<T>(T data) => new()
        {
            Success = true,
            Data = data,
            Message = null
        };
    }
}
