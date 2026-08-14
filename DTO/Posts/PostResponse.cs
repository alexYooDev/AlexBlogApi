namespace BlogApi.DTOs.Posts;

public class PostResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; } 
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public bool IsPublished { get; set; }

    public string AuthorName { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();

}