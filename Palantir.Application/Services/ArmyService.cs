namespace Palantir.Application.Services
{
    public class ArmyService : IArmyService
    {
        private readonly IArmyRepository _repository;

        public ArmyService(IArmyRepository repository) =>
            _repository = repository;

        public async Task<ArmyResponse?> GetByIdAsync(int id)
        {
            var army = await _repository.GetByIdAsync(id);
            if (army == null)
                return null;

            return Response(army);
        }

        public async Task<List<ArmyMapResponse>> GetByWarDateWithPositionsAsync(int warId, DateOnly date)
        {
            var armies = await _repository.GetByWarDateWithPositionsAsync(warId, date);

            return armies.Select(army => MapResponse(army)).ToList();
        }

        public async Task<ArmyResponse> AddAsync(CreateArmyRequest armyRequest)
        {
            var army = new Army
            {
                war_side_id = armyRequest.WarSideId,
                name_arm = armyRequest.Name,
                type_arm = armyRequest.TypeArmy,
                start_date = armyRequest.StartDate,
                end_date = armyRequest.EndDate,
                summary = armyRequest.Summary,
            };

            await _repository.AddAsync(army);

            return Response(army);
        }

        public async Task<ArmyMapResponse> AddWithPositionAsync(CreateArmyWithPositionRequest armyRequest)
        {
            armyRequest.Coordinate.SRID = 4326;

            var army = new Army
            {
                war_side_id = armyRequest.WarSideId,
                name_arm = armyRequest.Name,
                type_arm = armyRequest.TypeArmy,
                summary = armyRequest.Summary
            };

            var position = new ArmyPosition
            {
                date_position = armyRequest.DatePosition,
                coordinate = armyRequest.Coordinate,
                note = armyRequest.PositionNote
            };

            await _repository.AddWithPositionAsync(army, position);

            army.army_positions.Add(position);

            return MapResponse(army);
        }

        public async Task<ArmyMapResponse?> UpdatePositionAsync(int armyId, UpdateArmyMapPositionRequest positionRequest)
        {
            positionRequest.Coordinate.SRID = 4326;

            var position = new ArmyPosition
            {
                army_id = armyId,
                date_position = positionRequest.DatePosition,
                coordinate = positionRequest.Coordinate,
                note = positionRequest.Note
            };

            var army = await _repository.UpsertPositionAsync(armyId, position);

            return army is null
                ? null
                : MapResponse(army, positionRequest.DatePosition);
        }

        public async Task<ArmyResponse> UpdateAsync(int id, UpdateArmyRequest armyRequest)
        {
            var army = await _repository.GetByIdAsync(id);
            if (army == null)
                Exception();

            army.name_arm = armyRequest.Name;
            army.type_arm = armyRequest.TypeArmy;
            army.start_date = armyRequest.StartDate;
            army.end_date = armyRequest.EndDate;
            army.summary = armyRequest.Summary;

            await _repository.UpdateAsync(army);

            return Response(army);
        }

        public async Task DeleteAsync(int id)
        {
            var army = await _repository.GetByIdAsync(id);
            if (army == null)
                Exception();

            await _repository.DeleteAsync(id);
        }

        private ArmyResponse Response(Army army) =>
            new ArmyResponse(
                army.army_id,
                army.war_side_id,
                army.name_arm,
                army.type_arm,
                army.start_date,
                army.end_date,
                army.summary
            );

        private ArmyMapResponse MapResponse(Army army, DateOnly? datePosition = null)
        {
            var positionQuery = army.army_positions.AsEnumerable();

            if (datePosition is not null)
            {
                positionQuery = positionQuery
                    .Where(armyPosition => armyPosition.date_position == datePosition);
            }

            var position = positionQuery
                .OrderByDescending(armyPosition => armyPosition.date_position)
                .First();

            return new ArmyMapResponse(
                army.army_id,
                army.war_side_id,
                army.war_side?.side?.title,
                army.name_arm,
                army.type_arm,
                army.summary,
                position.army_position_id,
                position.date_position,
                position.coordinate,
                position.note
            );
        }

        private void Exception() =>
            throw new KeyNotFoundException("Army not found");
    }
}
