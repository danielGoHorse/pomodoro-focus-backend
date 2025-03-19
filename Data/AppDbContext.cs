using Microsoft.EntityFrameworkCore;
using Pomodoro.Api.Models;

namespace Pomodoro.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<PomodoroSession> PomodoroSessions { get; set; }
    }
}
