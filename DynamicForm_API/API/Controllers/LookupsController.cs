using DynamicForm_API.API.DTOs.Common;
using DynamicForm_API.API.DTOs.Lookups;
using DynamicForm_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DynamicForm_API.API.Controllers
{
    [ApiController]
    [Route("api/lookups")]
    public class LookupsController : ControllerBase
    {
        private readonly ILookupService _lookupService;

        public LookupsController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        [HttpGet("{lookupCode}/items")]
        public async Task<ActionResult<ApiSuccessEnvelopeDto<LookupOptionsResponseDto>>> GetLookupItems(
            string lookupCode,
            [FromQuery] bool activeOnly = true,
            CancellationToken cancellationToken = default)
        {
            if (!activeOnly)
            {
                return BadRequest(Problem(title: "Invalid request", detail: "Only activeOnly=true is currently supported.", statusCode: StatusCodes.Status400BadRequest));
            }

            try
            {
                var result = await _lookupService.GetLookupOptionsAsync(lookupCode, cancellationToken);
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
