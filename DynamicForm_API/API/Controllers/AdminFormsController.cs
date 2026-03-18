using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Forms;
using DynamicForm_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DynamicForm_API.API.Controllers
{
    [ApiController]
    [Route("api/admin/forms")]
    public class AdminFormsController : ControllerBase
    {
        private readonly IFormService _formService;

        public AdminFormsController(IFormService formService)
        {
            _formService = formService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<FormDetailResponseDto>>> CreateForm(
            [FromBody] CreateFormDraftRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _formService.CreateFormAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetFormById), new { formId = result.Id }, Success(result));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(Problem(title: "Invalid request", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(Problem(title: "Operation failed", detail: ex.Message, statusCode: StatusCodes.Status409Conflict));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<PagedResponseDto<FormSummaryResponseDto>>>> GetForms(
            [FromQuery] PagedRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _formService.GetFormsAsync(request, cancellationToken);
            return Ok(Success(result));
        }

        [HttpGet("{formId:int}")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<FormDetailResponseDto>>> GetFormById(
            int formId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _formService.GetFormByIdAsync(formId, cancellationToken);
                return Ok(Success(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Problem(title: "Not found", detail: ex.Message, statusCode: StatusCodes.Status404NotFound));
            }
        }

        [HttpPut("{formId:int}")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<FormDetailResponseDto>>> UpdateFormMetadata(
            int formId,
            [FromBody] UpdateFormMetadataRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _formService.UpdateFormMetadataAsync(formId, request, cancellationToken);
                return Ok(Success(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Problem(title: "Not found", detail: ex.Message, statusCode: StatusCodes.Status404NotFound));
            }
        }

        [HttpPost("{formId:int}/archive")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<FormDetailResponseDto>>> ArchiveForm(
            int formId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _formServiceArchive(formId, cancellationToken);
                return Ok(Success(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Problem(title: "Not found", detail: ex.Message, statusCode: StatusCodes.Status404NotFound));
            }
        }

        private async Task<FormDetailResponseDto> _formServiceArchive(int formId, CancellationToken cancellationToken)
        {
            return await _formService.ArchiveFormAsync(formId, cancellationToken);
        }

        private static ApiSuccessEnvelopeDto<T> Success<T>(T data) => new()
        {
            Success = true,
            Data = data,
            Message = null
        };
    }
}
