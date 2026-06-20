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

        [HttpGet("/api/operations/{operationId:int}/sides")]
        public async Task<ActionResult<List<MapSideResponse>>> GetSides(int operationId)
        {
            var sides = await _operationService.GetSidesAsync(operationId);

            return sides is null
                ? NotFound(new { message = "Операция не найдена." })
                : Ok(sides);
        }

        [HttpGet("by-theater/{theaterId:int}")]
        public async Task<ActionResult<IEnumerable<OperationResponse>>>
            GetByTheaterId(int theaterId)
        {
            var operations = await _operationService
                .GetByTheaterIdAsync(theaterId);

            return Ok(operations);
        }

        [HttpPost]
        public async Task<ActionResult> Add(OperationRequest operationRequest)
        {
            if (operationRequest.StartDate > operationRequest.EndDate)
                return BadRequest(new { message = "Дата начала операции не может быть позже даты окончания." });

            try
            {
                var response = await _operationService.AddAsync(operationRequest);
                return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Театр для операции не найден." });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, OperationRequest operationRequest)
        {
            if (operationRequest.StartDate > operationRequest.EndDate)
                return BadRequest(new { message = "Дата начала операции не может быть позже даты окончания." });

            try
            {
                await _operationService.UpdateAsync(id, operationRequest);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Операция или связанный театр не найдены." });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _operationService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Операция не найдена." });
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Нельзя удалить операцию: сначала удалите связанные события и стороны операции." });
            }
        }
    }
}
