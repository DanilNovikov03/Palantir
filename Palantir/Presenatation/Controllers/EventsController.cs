namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService) =>
            _eventService = eventService;

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventResponse>> GetById(int id)
        {
            var mapEvent = await _eventService.GetByIdAsync(id);

            return mapEvent is null
                ? NotFound()
                : Ok(mapEvent);
        }

        [HttpGet("by-operation/{operationId:int}")]
        public async Task<ActionResult<List<EventResponse>>> GetByOperation(int operationId)
        {
            var events = await _eventService.GetByOperationAsync(operationId);

            return Ok(events);
        }

        [HttpGet("by-war/{warId:int}")]
        public async Task<ActionResult<List<EventResponse>>> GetByWarDate(
            int warId,
            [FromQuery] DateOnly date,
            [FromQuery] int? operationId)
        {
            var events = await _eventService.GetByWarDateAsync(warId, date, operationId);

            return Ok(events);
        }

        [HttpPost]
        public async Task<ActionResult<EventResponse>> Add(EventRequest eventRequest)
        {
            var response = await _eventService.AddAsync(eventRequest);

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Id },
                response
            );
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<EventResponse>> Update(int id, EventRequest eventRequest)
        {
            var response = await _eventService.UpdateAsync(id, eventRequest);

            return response is null
                ? NotFound()
                : Ok(response);
        }

        [HttpPut("{id:int}/position")]
        public async Task<ActionResult<EventResponse>> UpdatePosition(
            int id,
            EventPositionRequest positionRequest)
        {
            var response = await _eventService.UpdatePositionAsync(id, positionRequest);

            return response is null
                ? NotFound()
                : Ok(response);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _eventService.DeleteAsync(id);

            return NoContent();
        }
    }
}
