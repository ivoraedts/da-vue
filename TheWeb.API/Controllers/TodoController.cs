using Microsoft.AspNetCore.Mvc;
using TheWebApi.Models;

namespace TheWebApi.Controllers;

[ApiController]
[Route("api/[controller]")] // This makes the URL: api/todo
public class TodoController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<TodoItem>> Get()
    {
        // This is your hardcoded list
        var items = new List<TodoItem>
        {
            new TodoItem { Id = 1, Task = "Learn Vue with .NET", IsCompleted = false },
            new TodoItem { Id = 2, Task = "Configure VS Code", IsCompleted = true },
            new TodoItem { Id = 3, Task = "Build an Awesome App", IsCompleted = false }
        };

        return Ok(items); // Returns a 200 OK status with the JSON list
    }
}