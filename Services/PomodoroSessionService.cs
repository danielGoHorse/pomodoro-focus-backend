// Services/PomodoroSessionService.cs
using Pomodoro.Api.Models;

public interface IPomodoroSessionService
{
    Task<IEnumerable<PomodoroSessionDto>> GetAllAsync();
    Task<PomodoroSessionDto> GetByIdAsync(int id);
    Task AddAsync(PomodoroSessionDto dto);
}

public class PomodoroSessionService : IPomodoroSessionService
{
    private readonly IPomodoroSessionRepository _repository;

    public PomodoroSessionService(IPomodoroSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PomodoroSessionDto>> GetAllAsync()
    {
        var sessions = await _repository.GetAllAsync();

        return sessions.Select(s => new PomodoroSessionDto
        {
            Id = s.Id,
            // Description = s.Description ?? string.Empty,
            CompletedAt = s.CompletedAt
        });
    }

    public async Task<PomodoroSessionDto> GetByIdAsync(int id)
    {
        var session = await _repository.GetByIdAsync(id);

        if (session == null)
            return new PomodoroSessionDto { Id = 0, CompletedAt = DateTime.MinValue };

        return new PomodoroSessionDto
        {
            Id = session.Id,
            // Description = session.Description,
            CompletedAt = session.CompletedAt
        };
    }

    public async Task AddAsync(PomodoroSessionDto dto)
    {
        var session = new PomodoroSession
        {
            // Description = dto.Description,
            CompletedAt = dto.CompletedAt
        };

        await _repository.AddAsync(session);
    }
}
