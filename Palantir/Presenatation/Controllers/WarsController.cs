namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("api/war")]
    public class WarsController : ControllerBase
    {
        private readonly IWarService _warService;

        public WarsController(IWarService warService) =>
            _warService = warService;


        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarResponse>>> GetAll()
        {
            var wars = await _warService.GetAllAsync();
            return Ok(wars);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WarResponse>> GetById(int id)
        {
            var war = await _warService.GetByIdAsync(id);
            return war is null ? 
                NotFound() : Ok(war);
        }

        [HttpGet("/api/wars/{warId:int}/sides")]
        public async Task<ActionResult<List<MapSideResponse>>> GetSides(int warId)
        {
            var sides = await _warService.GetSidesAsync(warId);

            return sides is null
                ? NotFound(new { message = "Конфликт не найден." })
                : Ok(sides);
        }

        [HttpPost("/api/wars/{warId:int}/sides")]
        public async Task<ActionResult<MapSideResponse>> AddSide(int warId, AddWarSideRequest request)
        {
            try
            {
                var response = await _warService.AddSideAsync(warId, request);
                return Created($"/api/wars/{warId}/sides/{response.WarSideId}", response);
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }

        [HttpDelete("/api/wars/{warId:int}/sides/{warSideId:int}")]
        public async Task<ActionResult> DeleteSide(int warId, int warSideId)
        {
            try
            {
                await _warService.DeleteSideAsync(warId, warSideId);
                return NoContent();
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Нельзя удалить сторону конфликта, пока она используется объектами карты." });
            }
        }

        [HttpPost]
        public async Task<ActionResult> Add(WarRequest warRequest)
        {
            if (warRequest.EndDate is not null && warRequest.StartDate > warRequest.EndDate)
                return BadRequest(new { message = "Дата начала конфликта не может быть позже даты окончания." });

            var response = await _warService.AddAsync(warRequest);
            return CreatedAtAction(
                nameof(GetById), 
                new { id = response.Id }, 
                response
            );
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, WarRequest warRequest)
        {
            if (warRequest.EndDate is not null && warRequest.StartDate > warRequest.EndDate)
                return BadRequest(new { message = "Дата начала конфликта не может быть позже даты окончания." });

            var response = await _warService
                .UpdateAsync(id, warRequest);
            return response is null ?
                NotFound() : NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _warService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Конфликт не найден." });
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "Нельзя удалить конфликт: сначала удалите связанные театры, события, зоны контроля и стороны." });
            }
        }
    }
}
