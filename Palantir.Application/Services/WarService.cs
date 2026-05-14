namespace Palantir.Application.Services
{
    public class WarService : IWarService
    {
        IWarRepository _repository;

        public WarService(IWarRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<WarResponse>> GetAllAsync()
        {
            var wars = await _repository.GetAllAsync();

            return wars.Select(war => new WarResponse(
                war.war_id,
                war.title,
                war.start_date,
                war.end_date,
                war.summary
            )).ToList();
        }

        public async Task<WarResponse?> GetByIdAsync(int id)
        {
            var war = await _repository.GetByIdAsync(id);
            if (war == null)
                return null;

            return new WarResponse(
                war.war_id,
                war.title,
                war.start_date,
                war.end_date,
                war.summary
            );
        }

        public async Task AddAsync(WarRequest warRequest)
        {
            var war = new War
            {
                title = warRequest.Title,
                start_date = warRequest.StartDate,
                end_date = warRequest.EndDate,
                summary = warRequest.Summary
            };

            await _repository.AddAsync(war);
        }

        public async Task UpdateAsync(int id, WarRequest warRequest)
        {
            var war = await _repository.GetByIdAsync(id);
            if (war == null)
                Exception();

            war.title = warRequest.Title;
            war.start_date = warRequest.StartDate;
            war.end_date = warRequest.EndDate;
            war.summary = warRequest.Summary;

            await _repository.UpdateAsync(war);
        }

        public async Task DeleteAsync(int id)
        {
            var war = await _repository.GetByIdAsync(id);
            if (war == null)
                Exception();

            await _repository.DeleteAsync(id);
        }

        private void Exception() =>
            throw new KeyNotFoundException("war not found");
    }
}
