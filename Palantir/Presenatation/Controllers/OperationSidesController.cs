namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OperationSidesController : ControllerBase
    {
        private readonly IOperationSideRepository _operationSideRep;

        public OperationSidesController(IOperationSideRepository operationSideRep)
        {
            _operationSideRep = operationSideRep;
        }
    }
}
