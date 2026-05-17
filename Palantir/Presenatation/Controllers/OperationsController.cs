namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OperationsController : ControllerBase
    {
        private readonly IOperationService _operationService;

        public OperationsController(IOperationService operationService) =>
            _operationService = operationService;


        [HttpGet("{id}")]
        public async Task<ActionResult<Operation>> GetById(int id)
        {
            var operation = await _operationService.GetByIdAsync(id);
            return operation is null ? 
                NotFound() : Ok(operation);
        }

        [HttpPost]
        public async Task<ActionResult> Add(OperationRequest operationRequest)
        {
            var response = await _operationService.AddAsync(operationRequest);
            return CreatedAtAction(nameof(GetById), new { id = response.Id });
        }

        [HttpPut]
        public async Task<ActionResult> Update(int id, OperationRequest operationRequest)
        {
            var response = await _operationService
                .UpdateAsync(id, operationRequest);
            return response is null ?
                NotFound() : NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            await _operationService.DeleteAsync(id);
            return NoContent();
        }
    }
}
