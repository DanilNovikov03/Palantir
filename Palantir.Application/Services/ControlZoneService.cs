namespace Palantir.Application.Services
{
    public class ControlZoneService : IControlZoneService
    {
        private readonly IControlZoneRepository _repository;

        public ControlZoneService(IControlZoneRepository repository) =>
            _repository = repository;

        public async Task<ControlZoneResponse?> GetByIdAsync(int controlZoneId)
        {
            var zone = await _repository
                .GetByIdAsync(controlZoneId);
            
            return zone is null ?
                null : Response(zone);
        }

        public async Task<List<ControlZoneResponse>> GetByWarDateAsync(int warId, DateOnly date)
        {
            var zones = await _repository
                .GetByWarDateAsync(warId, date);

            return zones.Select(zone =>
                    Response(zone)
                ).ToList();
        }

        public async Task<List<ControlZoneResponse>> GetByWarSideDateAsync(int warId, int warSideId, DateOnly date)
        {
            var zones = await _repository
                .GetByWarSideDateAsync(warId, warSideId, date);

            return zones.Select(zone =>
                Response(zone)
            ).ToList();
        }

        public async Task<ControlZoneResponse> AddAsync(CreateControlZoneRequest controlZoneRequest)
        {
            var request = controlZoneRequest;
            request.Geom.SRID = 4326;
            var zone = new ControlZone
            {
                war_id = request.WarId,
                war_side_id = request.WarSideId,
                date_control = request.DateControl,
                precision_control = request.PrecisionControl,
                geom = request.Geom
            };

            await _repository.AddAsync(zone);

            return Response(zone);
        }

        public async Task<ControlZoneResponse> UpdateAsync(int controlZoneId, UpdateControlZoneRequest controlZoneRequest)
        {
            var request = controlZoneRequest;
            var zone = await _repository
                .GetByIdAsync(controlZoneId);

            if (zone == null)
                Exception();

            zone.precision_control = request.PrecisionControl;
            zone.geom = request.Geom;

            await _repository.UpdateAsync(zone);

            return Response(zone);
        }

        public async Task<ControlZoneGeometryForDateResponse> UpdateGeometryForDateAsync(
            int controlZoneId,
            UpdateControlZoneGeometryForDateRequest request)
        {
            var sourceZone = await _repository.GetByIdAsync(controlZoneId);
            if (sourceZone == null)
                throw new KeyNotFoundException("Зона контроля не найдена.");

            ValidateGeometryForDateRequest(request);

            var geometry = (MultiPolygon)request.Geom!.Copy();
            geometry.SRID = 4326;

            if (sourceZone.date_control == request.Date)
            {
                sourceZone.geom = geometry;
                await _repository.UpdateAsync(sourceZone);

                return new ControlZoneGeometryForDateResponse(Response(sourceZone), false);
            }

            var newZone = new ControlZone
            {
                war_id = sourceZone.war_id,
                war_side_id = sourceZone.war_side_id,
                date_control = request.Date,
                precision_control = sourceZone.precision_control,
                geom = geometry
            };

            await _repository.AddAsync(newZone);

            return new ControlZoneGeometryForDateResponse(Response(newZone), true);
        }

        public async Task DeleteAsync(int controlZoneId)
        {
            var zone = await _repository
                .GetByIdAsync(controlZoneId);
            if (zone == null)
                Exception();

            await _repository.DeleteAsync(controlZoneId);
        }

        private ControlZoneResponse Response(ControlZone zone) =>
            new ControlZoneResponse(
                zone.control_id,
                zone.war_id,
                zone.war_side_id,
                zone.date_control,
                zone.precision_control,
                zone.geom
            );

        private static void ValidateGeometryForDateRequest(UpdateControlZoneGeometryForDateRequest request)
        {
            if (request.Date == default)
                throw new ArgumentException("Передана некорректная дата.");

            if (request.Geom == null || request.Geom.IsEmpty)
                throw new ArgumentException("Некорректная геометрия зоны контроля.");

            for (var index = 0; index < request.Geom.NumGeometries; index++)
            {
                var polygon = (Polygon)request.Geom.GetGeometryN(index);
                if (polygon.ExteriorRing.NumPoints < 4)
                    throw new ArgumentException("Для зоны контроля нужно минимум 3 точки.");
            }

            if (!request.Geom.IsValid)
                throw new ArgumentException("Некорректная геометрия зоны контроля.");
        }

        private void Exception() =>
            throw new KeyNotFoundException("control zone not found");
    }
}
