namespace TheWebApi.Models
{
    public class TodoItem
    {
        public long Id { get; set; }
        public string? Task { get; set; }
        public bool IsCompleted { get; set; }
    }
}