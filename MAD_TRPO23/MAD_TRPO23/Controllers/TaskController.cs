using MAD_TRPO23.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MAD_TRPO23.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly MadTrpo23Context _context;

    public TaskController(MadTrpo23Context context)
    {
        _context = context;
    }

    public class TaskRequest
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool IsCompleted { get; set; }
    }

    [HttpGet]
    public IActionResult GetMyTasks()
    {
        var login = User.Identity?.Name;
        if (login == null)
            return Unauthorized();

        var tasks = _context.Tasks
            .Where(x => x.OwnerLogin == login)
            .ToList();

        return Ok(tasks);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetAllTasks()
    {
        return Ok(_context.Tasks.ToList());
    }

    [HttpGet("{id:int}")]
    public IActionResult GetTask(int id)
    {
        var task = _context.Tasks.Find(id);
        if (task == null)
            return NotFound();

        if (!User.IsInRole("Admin") && task.OwnerLogin != User.Identity?.Name)
            return Forbid();

        return Ok(task);
    }

    [HttpPost]
    public IActionResult AddTask(TaskRequest request)
    {
        var login = User.Identity?.Name;
        if (login == null)
            return Unauthorized();

        var task = new Models.Task();
        task.Title = request.Title;
        task.Description = request.Description;
        task.OwnerLogin = login;
        task.IsCompleted = request.IsCompleted;

        _context.Tasks.Add(task);
        _context.SaveChanges();

        return Ok(task);
    }

    [HttpPut("{id:int}")]
    public IActionResult EditTask(int id, TaskRequest request)
    {
        var task = _context.Tasks.Find(id);
        if (task == null)
            return NotFound();

        if (!User.IsInRole("Admin") && task.OwnerLogin != User.Identity?.Name)
            return Forbid();

        task.Title = request.Title;
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;

        _context.SaveChanges();
        return Ok(task);
    }

    [HttpDelete("{id:int}")]
    public IActionResult DeleteTask(int id)
    {
        var task = _context.Tasks.Find(id);
        if (task == null)
            return NotFound();

        if (!User.IsInRole("Admin") && task.OwnerLogin != User.Identity?.Name)
            return Forbid();

        _context.Tasks.Remove(task);
        _context.SaveChanges();

        return Ok("Задача удалена");
    }
}
