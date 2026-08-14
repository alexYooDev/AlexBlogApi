namespace BlogApi.DTOs.Authors;

public class AuthorResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Email { get; set; }
}