namespace Pomodoro.Api.Models
{
    public class PomodoroSession
    {
        public int Id { get; set; }
        public required string Description { get; set; }
        public DateTime CompletedAt { get; set; }
    }


}




