// Repositories/IPomodoroSessionRepository.cs
using Pomodoro.Api.Models;

public interface IPomodoroSessionRepository
{
    Task<IEnumerable<PomodoroSession>> GetAllAsync();
    Task<PomodoroSession> GetByIdAsync(int id);
    Task AddAsync(PomodoroSession session);
}
