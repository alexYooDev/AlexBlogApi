namespace BlogApi.DTOs.Posts;

/* Client -> Server Data format : Blocks manual injection of id and createdAt */
public class CreatePostRequest
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public bool IsPublished { get; set; } = false;

    public int AuthorId { get; set; }
    public List<string> TagNames { get; set; } = new();
}