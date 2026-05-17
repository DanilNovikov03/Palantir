namespace Palantir.Application.Services
{
    public class WarService : IWarService
    {
        IWarRepository _repository;

        public WarService(IWarRepository repository) =>
            _repository = repository;


        public async Task<List<WarResponse>> GetAllAsync()
        {
            var wars = await _repository.GetAllAsync();

            return wars.Select(war => 
                Response(war)
            ).ToList();
        }

        public async Task<WarResponse?> GetByIdAsync(int id)
        {
            var war = await _repository.GetByIdAsync(id);
            if (war == null)
                return null;

            return Response(war);
        }

        public async Task<WarResponse> AddAsync(WarRequest warRequest)
        {
            var war = new War
            {
                title = warRequest.Title,
                start_date = warRequest.StartDate,
                end_date = warRequest.EndDate,
                summary = warRequest.Summary
            };

            await _repository.AddAsync(war);

            return Response(war);
        }

        public async Task<WarResponse> UpdateAsync(int id, WarRequest warRequest)
        {
            var war = await _repository.GetByIdAsync(id);
            if (war == null)
                Exception();

            war.title = warRequest.Title;
            war.start_date = warRequest.StartDate;
            war.end_date = warRequest.EndDate;
            war.summary = warRequest.Summary;

            await _repository.UpdateAsync(war);

            return Response(war);
        }

        public async Task DeleteAsync(int id)
        {
            var war = await _repository.GetByIdAsync(id);
            if (war == null)
                Exception();

            await _repository.DeleteAsync(id);
        }

        private WarResponse Response(War war) =>
            new WarResponse(
                war.war_id,
                war.title,
                war.start_date,
                war.end_date,
                war.summary
            );

        private void Exception() =>
            throw new KeyNotFoundException("war not found");
    }
}
