namespace BlogApi.DTOs.Posts;

public class UpdatePostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public bool IsPublished { get; set; }
    public List<string> TagNames { get; set; } = new();
}