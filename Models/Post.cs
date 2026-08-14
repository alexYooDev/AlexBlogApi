namespace BlogApi.Models;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug {get; set;} = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }

    /* separated for draft state */
    public bool IsPublished { get; set; } = false;

    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;
    public List<Tag> Tags { get; set; } = new();
}