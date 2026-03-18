using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Fields;
using DynamicForm_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DynamicForm_API.API.Controllers
{
    [ApiController]
    [Route("api/admin/forms/{formId:int}/versions/{versionId:int}/fields")]
    public class AdminFieldsController : ControllerBase
    {
        private readonly IFormVersionService _formVersionService;

        public AdminFieldsController(IFormVersionService formVersionService)
        {
            _formVersionService = formVersionService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<FormFieldResponseDto>>> AddField(
            int formId,
            int versionId,
            [FromBody] AddDraftFieldRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _formVersionService.AddDraftFieldAsync(formId, versionId, request, cancellationToken);
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

        [HttpPut("{fieldId:int}")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<FormFieldResponseDto>>> UpdateField(
            int formId,
            int versionId,
            int fieldId,
            [FromBody] UpdateDraftFieldRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _formVersionService.UpdateDraftFieldAsync(formId, versionId, fieldId, request, cancellationToken);
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

        [HttpDelete("{fieldId:int}")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<bool>>> RemoveField(
            int formId,
            int versionId,
            int fieldId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _formVersionService.RemoveDraftFieldAsync(formId, versionId, fieldId, cancellationToken);
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

        [HttpPut("reorder")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<IReadOnlyList<FormFieldResponseDto>>>> ReorderFields(
            int formId,
            int versionId,
            [FromBody] ReorderDraftFieldsRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _formVersionService.ReorderDraftFieldsAsync(formId, versionId, request, cancellationToken);
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
