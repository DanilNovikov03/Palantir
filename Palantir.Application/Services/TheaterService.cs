namespace Palantir.Application.Services
{
    public class TheaterService : ITheaterService
    {
        ITheaterRepository _repository;

        public TheaterService(ITheaterRepository repository) =>
            _repository = repository;

        public async Task<List<TheaterResponse>> GetAllAsync()
        {
            var theaters = await _repository.GetAllAsync();

            return theaters.Select(theater => 
                Response(theater)
            ).ToList();
        }

        public async Task<TheaterResponse?> GetByIdAsync(int id)
        {
            var theater = await _repository.GetByIdAsync(id);
            if (theater == null)
                return null;

            return Response(theater);
        }

        public async Task<TheaterResponse> AddAsync(TheaterRequest theaterRequest)
        {

            var theater = new Theater
            {
                title = theaterRequest.Title,
                summary = theaterRequest.Summary
            };

            await _repository.AddAsync(theater);

            return Response(theater);
        }

        public async Task<TheaterResponse> UpdateAsync(int theaterId, TheaterRequest request)
        {
            var theater = await _repository.GetByIdAsync(theaterId);
            if (theater == null)
                Exception();

            theater.title = request.Title;
            theater.summary = request.Summary;

            await _repository.UpdateAsync(theater);

            return Response(theater);
        }

        public async Task DeleteAsync(int theaterId)
        {
            var theater = await _repository.GetByIdAsync(theaterId);
            if (theater == null)
                Exception();

            await _repository.DeleteAsync(theaterId);
        }

        private TheaterResponse Response(Theater theater) =>
            new TheaterResponse(
                theater.theater_id,
                theater.war_id,
                theater.title,
                theater.summary
            );

        private void Exception() =>
            throw new KeyNotFoundException("Theater not found");
    }
}
