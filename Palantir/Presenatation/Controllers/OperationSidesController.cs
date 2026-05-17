namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OperationSidesController : ControllerBase
    {
        private readonly IOperationSideService _operationSideService;

        public OperationSidesController(IOperationSideService operationSideService) =>
            _operationSideService = operationSideService;


        [HttpGet("{operationId:int}/{warSideId:int}")]
        public async Task<ActionResult<OperationSide>> GetByIds(int operationId, int warSideId)
        {
            var operSide = await _operationSideService
                    .GetByIdsAsync(operationId, warSideId);
            return operSide is null ?
                NotFound() : Ok(operSide);
        }

        [HttpGet("operation/{operationId:int}")]
        public async Task<ActionResult<OperationSide>> GetByOperationId(int operationId)
        {
            var operSide = await _operationSideService
                .GetByOperationIdAsync(operationId);
            return operSide is null ?
                NotFound() : Ok(operSide);
        }

        [HttpGet("warSides/{warSideId:int}")]
        public async Task<ActionResult<OperationSide>> GetByWarSideId(int warSideId)
        {
            var operSide = await _operationSideService
                .GetByWarSideIdAsync(warSideId);
            return operSide is null ?
                NotFound() : Ok(operSide);
        }

        [HttpPost]
        public async Task<ActionResult> Add(CreateOperationSideRequest operationSideRequest)
        {
            await _operationSideService.AddAsync(operationSideRequest);
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> Update(
            int operationId, int warSideId, 
            UpdateOperationSideRequest operationSideRequest)
        {
            await _operationSideService
                .UpdateAsync(operationId, warSideId, operationSideRequest);
            return Ok();
        }

        [HttpDelete("{operationId:int}/{warSideId:int}")]
        public async Task<ActionResult> Delete(int operationId, int warSideId)
        {
            await _operationSideService
                .DeleteAsync(operationId, warSideId);
            return Ok();
        }
    }
}
