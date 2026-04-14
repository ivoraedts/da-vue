using Microsoft.AspNetCore.Mvc;
using TheWeb.API.Data;
using TheWebApi.Models;

namespace TheWebApi.Controllers;

[ApiController]
[Route("api/[controller]")] // This makes the URL: api/todo
public class TodoController : ControllerBase
{
    private readonly DaVueDbContext _dbContext;

    public TodoController(DaVueDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public ActionResult<List<Models.TodoItem>> Get()
    {
        Console.WriteLine("Received GET request for /api/todo");

        var todoItems = _dbContext.TodoItems.ToList();
        var items = todoItems.Select(item => new Models.TodoItem
        {
            Id = item.Id,
            Task = item.Task,
            IsCompleted = item.IsCompleted
        }).ToList();

        return Ok(items); // Returns a 200 OK status with the JSON list
    }
}