namespace Palantir.Application.Services
{
    public class ArmyPositionServices : IArmyPositionService
    {
        private readonly IArmyPositionRepository _repository;

        public ArmyPositionServices(IArmyPositionRepository armyPositionRepository) =>
            _repository = armyPositionRepository;


        public async Task<ArmyPositionResponse> AddAsync(CreateArmyPositionRequest armyPosRequest)
        {
            var armyPos = new ArmyPosition
            {
                army_id = armyPosRequest.ArmyId,
                date_position = armyPosRequest.DatePosition,
                coordinate = armyPosRequest.Coordinate,
                note = armyPosRequest.Note
            };

            await _repository.AddAsync(armyPos);

            return Response(armyPos);
        }

        public async Task<ArmyPositionResponse> UpdateAsync(int id, UpdateArmyPositionRequest armyPosRequest)
        {
            var armyPos = await _repository.GetByIdAsync(id);
            if (armyPos == null)
                Exception();

            armyPos.coordinate = armyPosRequest.Coordinate;
            armyPos.note = armyPosRequest.Note;

            await _repository.UpdateAsync(armyPos);

            return Response(armyPos);
        }

        public async Task DeleteAsync(int id)
        {
            var armyPos = await _repository.GetByIdAsync(id);
            if (armyPos == null)
                Exception();

            await _repository.DeleteAsync(id);
        }

        private ArmyPositionResponse Response(ArmyPosition armyPosition) =>
            new ArmyPositionResponse(
                armyPosition.army_position_id,
                armyPosition.army_id,
                armyPosition.date_position,
                armyPosition.coordinate,
                armyPosition.note
            );

        private void Exception() =>
            throw new KeyNotFoundException("ArmyPosition not found");
    }
}
