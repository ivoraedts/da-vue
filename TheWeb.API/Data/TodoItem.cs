using System;

namespace TheWeb.API.Data;

public class TodoItem
{
    public long Id { get; set; }
    public required string Task { get; set; }
    public bool IsCompleted { get; set; }
}
