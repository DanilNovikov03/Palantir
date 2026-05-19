namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("api/operation")]
    public class OperationsController : ControllerBase
    {
        private readonly IOperationService _operationService;

        public OperationsController(IOperationService operationService) =>
            _operationService = operationService;


        [HttpGet("{id:int}")]
        public async Task<ActionResult<OperationResponse>> GetById(int id)
        {
            var operation = await _operationService.GetByIdAsync(id);
            return operation is null ? 
                NotFound() : Ok(operation);
        }

        [HttpGet("by-theater/{theaterId:int}")]
        public async Task<ActionResult<IEnumerable<TheaterResponse>>>
            GetByTheaterId(int theaterId)
        {
            var operations = await _operationService
                .GetByTheaterIdAsync(theaterId);

            return Ok(operations);
        }

        [HttpPost]
        public async Task<ActionResult> Add(OperationRequest operationRequest)
        {
            var response = await _operationService.AddAsync(operationRequest);
            return CreatedAtAction(nameof(GetById), new { id = response.Id });
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, OperationRequest operationRequest)
        {
            var response = await _operationService
                .UpdateAsync(id, operationRequest);
            return response is null ?
                NotFound() : NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _operationService.DeleteAsync(id);
            return NoContent();
        }
    }
}
