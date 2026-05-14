namespace Palantir.Application.Services
{
    public class ArmyPositionServices : IArmyPositionService
    {
        private readonly IArmyPositionRepository _repository;

        public ArmyPositionServices(IArmyPositionRepository armyPositionRepository)
        {
            _repository = armyPositionRepository;
        }

        public async Task AddAsync(CreateArmyPositionRequest armyPosRequest)
        {
            var armyPos = new ArmyPosition
            {
                army_id = armyPosRequest.ArmyId,
                date_position = armyPosRequest.DatePosition,
                coordinate = armyPosRequest.Coordinate,
                note = armyPosRequest.Note
            };

            await _repository.AddAsync(armyPos);
        }

        public async Task UpdateAsync(int id, UpdateArmyPositionRequest armyPosRequest)
        {
            var armyPos = await _repository.GetByIdAsync(id);
            if (armyPos == null)
                Exception();

            armyPos.coordinate = armyPosRequest.Coordinate;
            armyPos.note = armyPosRequest.Note;

            await _repository.UpdateAsync(armyPos);
        }

        public async Task DeleteAsync(int id)
        {
            var armyPos = await _repository.GetByIdAsync(id);
            if (armyPos == null)
                Exception();

            await _repository.DeleteAsync(id);
        }

        private void Exception() =>
            throw new KeyNotFoundException("ArmyPosition not found");
    }
}
