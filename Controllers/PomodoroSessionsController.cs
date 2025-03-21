// Controllers/PomodoroSessionController.cs
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/pomodorosession")]
public class PomodoroSessionController : ControllerBase
{
    private readonly IPomodoroSessionService _service;

    public PomodoroSessionController(IPomodoroSessionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PomodoroSessionDto>>> GetAll()
    {
        var sessions = await _service.GetAllAsync();

        if (sessions == null)
            sessions = new List<PomodoroSessionDto>();

        return Ok(sessions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PomodoroSessionDto>> Get(int id)
    {
        var session = await _service.GetByIdAsync(id);

        if (session == null)
            return NotFound();

        return Ok(session);
    }

    [HttpPost]
    public async Task<ActionResult> Post(PomodoroSessionDto dto)
    {
        await _service.AddAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
    }
}
