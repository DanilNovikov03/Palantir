namespace Palantir.Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _repository;

        public EventService(IEventRepository repository) =>
            _repository = repository;

        public async Task<EventResponse?> GetByIdAsync(int id)
        {
            var mapEvent = await _repository.GetByIdAsync(id);

            return mapEvent is null
                ? null
                : Response(mapEvent);
        }

        public async Task<List<EventResponse>> GetByOperationAsync(int operationId)
        {
            var events = await _repository.GetByOperationAsync(operationId);

            return events
                .Select(Response)
                .ToList();
        }

        public async Task<List<EventResponse>> GetByWarDateAsync(int warId, DateOnly date, int? operationId)
        {
            var events = await _repository.GetByWarDateAsync(warId, date, operationId);

            return events
                .Select(Response)
                .ToList();
        }

        public async Task<EventResponse> AddAsync(EventRequest eventRequest)
        {
            var mapEvent = new Event
            {
                war_id = eventRequest.WarId,
                operation_id = eventRequest.OperationId,
                war_side_id = eventRequest.WarSideId,
                title = eventRequest.Title,
                text_ev = eventRequest.Text,
                type_ev = eventRequest.Type,
                date_ev = eventRequest.Date,
                coordinate = CreatePoint(eventRequest.Latitude, eventRequest.Longitude)
            };

            await _repository.AddAsync(mapEvent);

            var createdEvent = await _repository.GetByIdAsync(mapEvent.event_id);

            return Response(createdEvent ?? mapEvent);
        }

        public async Task<EventResponse?> UpdateAsync(int id, EventRequest eventRequest)
        {
            var mapEvent = await _repository.GetByIdAsync(id);

            if (mapEvent is null)
            {
                return null;
            }

            mapEvent.war_id = eventRequest.WarId;
            mapEvent.operation_id = eventRequest.OperationId;
            mapEvent.war_side_id = eventRequest.WarSideId;
            mapEvent.title = eventRequest.Title;
            mapEvent.text_ev = eventRequest.Text;
            mapEvent.type_ev = eventRequest.Type;
            mapEvent.date_ev = eventRequest.Date;
            mapEvent.coordinate = CreatePoint(eventRequest.Latitude, eventRequest.Longitude);

            await _repository.UpdateAsync(mapEvent);

            var updatedEvent = await _repository.GetByIdAsync(id);

            return Response(updatedEvent ?? mapEvent);
        }

        public async Task<EventResponse?> UpdatePositionAsync(int id, EventPositionRequest positionRequest)
        {
            var mapEvent = await _repository.GetByIdAsync(id);

            if (mapEvent is null)
            {
                return null;
            }

            mapEvent.coordinate = CreatePoint(positionRequest.Latitude, positionRequest.Longitude);

            await _repository.UpdateAsync(mapEvent);

            return Response(mapEvent);
        }

        public async Task DeleteAsync(int id)
        {
            var mapEvent = await _repository.GetByIdAsync(id);

            if (mapEvent is null)
            {
                throw new KeyNotFoundException("event not found");
            }

            await _repository.DeleteAsync(id);
        }

        private static Point CreatePoint(double latitude, double longitude) =>
            new Point(longitude, latitude) { SRID = 4326 };

        private static EventResponse Response(Event mapEvent) =>
            new EventResponse(
                mapEvent.event_id,
                mapEvent.war_id,
                mapEvent.operation_id,
                mapEvent.war_side_id,
                mapEvent.title,
                mapEvent.text_ev,
                mapEvent.type_ev,
                mapEvent.date_ev,
                mapEvent.coordinate.Y,
                mapEvent.coordinate.X,
                mapEvent.war_side?.side?.title,
                null
            );
    }
}
