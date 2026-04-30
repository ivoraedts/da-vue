namespace TheWeb.API.Models;

public class Boundaries
{
    public required DateTime OldestItem { get; set; }
    public required DateTime NewestItem { get; set; }
}