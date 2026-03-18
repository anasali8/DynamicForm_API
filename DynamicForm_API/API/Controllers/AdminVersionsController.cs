using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Versions;
using DynamicForm_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DynamicForm_API.API.Controllers
{
    [ApiController]
    [Route("api/admin/forms/{formId:int}/versions")]
    public class AdminVersionsController : ControllerBase
    {
        private readonly IFormVersionService _formVersionService;

        public AdminVersionsController(IFormVersionService formVersionService)
        {
            _formVersionService = formVersionService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<FormVersionDetailResponseDto>>> CreateNextDraftVersion(
            int formId,
            [FromBody] CreateNextDraftVersionRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _formVersionService.CreateNextDraftVersionAsync(formId, request, cancellationToken);
                return Ok(Success(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Problem(title: "Not found", detail: ex.Message, statusCode: StatusCodes.Status404NotFound));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<PagedResponseDto<FormVersionHistoryItemResponseDto>>>> GetVersions(
            int formId,
            [FromQuery] PagedRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _formVersionService.GetVersionsByFormIdAsync(formId, request, cancellationToken);
                return Ok(Success(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Problem(title: "Not found", detail: ex.Message, statusCode: StatusCodes.Status404NotFound));
            }
        }

        [HttpGet("{versionId:int}")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<FormVersionDetailResponseDto>>> GetVersionDetails(
            int formId,
            int versionId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _formVersionService.GetVersionDetailsAsync(formId, versionId, cancellationToken);
                return Ok(Success(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Problem(title: "Not found", detail: ex.Message, statusCode: StatusCodes.Status404NotFound));
            }
        }

        [HttpPut("{versionId:int}")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<FormVersionDetailResponseDto>>> UpdateDraftVersion(
            int formId,
            int versionId,
            [FromBody] UpdateDraftVersionRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _formVersionService.UpdateDraftVersionAsync(formId, versionId, request, cancellationToken);
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

        [HttpPost("{versionId:int}/publish")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<FormVersionDetailResponseDto>>> PublishVersion(
            int formId,
            int versionId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _formVersionService.PublishDraftVersionAsync(formId, versionId, cancellationToken);
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

        [HttpPost("{versionId:int}/archive")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<FormVersionDetailResponseDto>>> ArchiveVersion(
            int formId,
            int versionId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _formVersionService.ArchiveVersionAsync(formId, versionId, cancellationToken);
                return Ok(Success(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Problem(title: "Not found", detail: ex.Message, statusCode: StatusCodes.Status404NotFound));
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
