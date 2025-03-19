// Repositories/PomodoroSessionRepository.cs
using Microsoft.EntityFrameworkCore;
using Pomodoro.Api.Data;
using Pomodoro.Api.Models;

public class PomodoroSessionRepository : IPomodoroSessionRepository
{
    private readonly AppDbContext _context;

    public PomodoroSessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PomodoroSession>> GetAllAsync() =>
        await _context.PomodoroSessions.ToListAsync();

    public async Task<PomodoroSession> GetByIdAsync(int id)
    {
        var session = await _context.PomodoroSessions.FindAsync(id);

        if (session == null)
            throw new KeyNotFoundException($"Sessão com Id {id} não encontrada");

        return session;
    }

    public async Task AddAsync(PomodoroSession session)
    {
        _context.PomodoroSessions.Add(session);
        await _context.SaveChangesAsync();
    }
}
