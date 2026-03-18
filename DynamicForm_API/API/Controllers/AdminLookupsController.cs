using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Lookups;
using DynamicForm_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DynamicForm_API.API.Controllers
{
    [ApiController]
    [Route("api/admin/lookups")]
    public class AdminLookupsController : ControllerBase
    {
        private readonly ILookupService _lookupService;

        public AdminLookupsController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<LookupTableDetailResponseDto>>> CreateLookupTable(
            [FromBody] CreateLookupTableRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _lookupService.CreateLookupTableAsync(request, cancellationToken);
                return Ok(Success(result));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(Problem(title: "Operation failed", detail: ex.Message, statusCode: StatusCodes.Status409Conflict));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<PagedResponseDto<LookupTableSummaryResponseDto>>>> GetLookupTables(
            [FromQuery] PagedRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _lookupService.GetLookupTablesAsync(request, cancellationToken);
            return Ok(Success(result));
        }

        [HttpGet("{lookupId:int}")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<LookupTableDetailResponseDto>>> GetLookupById(
            int lookupId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _lookupService.GetLookupTableByIdAsync(lookupId, cancellationToken);
                return Ok(Success(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Problem(title: "Not found", detail: ex.Message, statusCode: StatusCodes.Status404NotFound));
            }
        }

        [HttpPut("{lookupId:int}")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<LookupTableDetailResponseDto>>> UpdateLookup(
            int lookupId,
            [FromBody] UpdateLookupTableRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _lookupService.UpdateLookupTableAsync(lookupId, request, cancellationToken);
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

        [HttpPost("{lookupId:int}/archive")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<LookupTableDetailResponseDto>>> ArchiveLookup(
            int lookupId,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _lookupService.ArchiveLookupTableAsync(lookupId, cancellationToken);
                return Ok(Success(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Problem(title: "Not found", detail: ex.Message, statusCode: StatusCodes.Status404NotFound));
            }
        }

        [HttpPost("{lookupId:int}/items")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<LookupItemResponseDto>>> CreateLookupItem(
            int lookupId,
            [FromBody] CreateLookupItemRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _lookupService.CreateLookupItemAsync(lookupId, request, cancellationToken);
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

        [HttpGet("{lookupId:int}/items")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<IReadOnlyList<LookupItemResponseDto>>>> GetLookupItems(
            int lookupId,
            CancellationToken cancellationToken)
        {
            try
            {
                var table = await _lookupService.GetLookupTableByIdAsync(lookupId, cancellationToken);
                return Ok(Success(table.Items));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Problem(title: "Not found", detail: ex.Message, statusCode: StatusCodes.Status404NotFound));
            }
        }

        [HttpPut("{lookupId:int}/items/{itemId:int}")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<LookupItemResponseDto>>> UpdateLookupItem(
            int lookupId,
            int itemId,
            [FromBody] UpdateLookupItemRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _lookupService.UpdateLookupItemAsync(lookupId, itemId, request, cancellationToken);
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

        [HttpPut("{lookupId:int}/items/{itemId:int}/active-state")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<LookupItemResponseDto>>> SetLookupItemActiveState(
            int lookupId,
            int itemId,
            [FromBody] SetLookupItemActiveStateRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _lookupService.SetLookupItemActiveStateAsync(lookupId, itemId, request, cancellationToken);
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

        [HttpPut("{lookupId:int}/items/reorder")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<IReadOnlyList<LookupItemResponseDto>>>> ReorderLookupItems(
            int lookupId,
            [FromBody] ReorderLookupItemsRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _lookupService.ReorderLookupItemsAsync(lookupId, request, cancellationToken);
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
