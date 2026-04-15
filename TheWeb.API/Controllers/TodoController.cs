using Microsoft.AspNetCore.Mvc;
using TheWeb.API.Data;

namespace TheWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
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

        var todoItems = _dbContext.TodoItems.OrderBy(item => item.Id).ToList();
        var items = todoItems.Select(item => new Models.TodoItem
        {
            Id = item.Id,
            Task = item.Task,
            IsCompleted = item.IsCompleted
        }).ToList();

        return Ok(items); // Returns a 200 OK status with the JSON list
    }

    [HttpPost]
    public ActionResult<Models.TodoItem> Post([FromBody] Models.TodoItem newItem)
    {
        if (newItem == null || string.IsNullOrEmpty(newItem.Task))
        {
            return BadRequest("Task is required."); // Return 400 Bad Request if the input is invalid
        }

        var todoItem = new TheWeb.API.Data.TodoItem
        {
            Task = newItem.Task,
            IsCompleted = newItem.IsCompleted
        };

        _dbContext.TodoItems.Add(todoItem);
        _dbContext.SaveChanges();

        // Return the created item with its new ID
        return CreatedAtAction(nameof(Get), new { id = todoItem.Id }, new Models.TodoItem
        {
            Id = todoItem.Id,
            Task = todoItem.Task,
            IsCompleted = todoItem.IsCompleted
        });
    }

    [HttpPut("{id}")]
    public ActionResult<Models.TodoItem> Put(int id, [FromBody] Models.TodoItem updatedItem)
    {
        if (updatedItem == null || string.IsNullOrEmpty(updatedItem.Task))
        {
            return BadRequest("Task is required.");
        }

        var todoItem = _dbContext.TodoItems.SingleOrDefault(t => t.Id == id);
        if (todoItem == null)
        {
            return NotFound();
        }

        todoItem.Task = updatedItem.Task;
        todoItem.IsCompleted = updatedItem.IsCompleted;

        _dbContext.SaveChanges();

        return Ok(new Models.TodoItem
        {
            Id = todoItem.Id,
            Task = todoItem.Task,
            IsCompleted = todoItem.IsCompleted
        });
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var todoItem = _dbContext.TodoItems.SingleOrDefault(t => t.Id == id);
        if (todoItem == null)
        {
            return NotFound();
        }

        _dbContext.TodoItems.Remove(todoItem);
        _dbContext.SaveChanges();

        return NoContent();
    }
}