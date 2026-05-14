namespace Palantir.Application.Services
{
    public class ArmyService : IArmyService
    {
        IArmyRepository _repository;

        public ArmyService(IArmyRepository repository)
        {
            _repository = repository;
        }

        public async Task<ArmyResponse?> GetByIdAsync(int id)
        {
            var army = await _repository.GetByIdAsync(id);
            if (army == null)
                return null;

            return new ArmyResponse(
                army.army_id,
                army.name_arm
            );
        }

        public async Task AddAsync(ArmyRequest armyRequest)
        {
            var army = new Army
            {
                name_arm = armyRequest.Name
            };

            await _repository.AddAsync(army);
        }

        public async Task UpdateAsync(int id, ArmyRequest armyRequest)
        {
            var army = await _repository.GetByIdAsync(id);
            if (army == null)
                Exception();

            army.name_arm = armyRequest.Name;

            await _repository.UpdateAsync(army);
        }

        public async Task DeleteAsync(int id)
        {
            var army = await _repository.GetByIdAsync(id);
            if (army == null)
                Exception();

            await _repository.DeleteAsync(id);
        }

        private void Exception() =>
            throw new KeyNotFoundException("Army not found");
    }
}
