// DTOs/PomodoroSessionDto.cs
public class PomodoroSessionDto
{
    public int Id { get; set; }
    public required string Description { get; set; }
    public DateTime CompletedAt { get; set; }
}
