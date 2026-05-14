namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OperationsController : ControllerBase
    {
        private readonly IOperationRepository _operationRep;

        public OperationsController(IOperationRepository operationRep)
        {
            _operationRep = operationRep;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Operation>>> GetAll() =>
            await _operationRep.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Operation>> GetById(int id)
        {
            var operation = await _operationRep.GetByIdAsync(id);
            return operation is null ? 
                NotFound() : Ok(operation);
        }
    }
}
