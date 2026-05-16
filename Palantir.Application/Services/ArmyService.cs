namespace Palantir.Application.Services
{
    public class ArmyService : IArmyService
    {
        IArmyRepository _repository;

        public ArmyService(IArmyRepository repository) =>
            _repository = repository;

        public async Task<ArmyResponse?> GetByIdAsync(int id)
        {
            var army = await _repository.GetByIdAsync(id);
            if (army == null)
                return null;

            return Response(army);
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

        private void Exception() =>
            throw new KeyNotFoundException("Army not found");
    }
}
