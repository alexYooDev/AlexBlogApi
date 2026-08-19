namespace BlogApi.DTOs.Authors;

public class UpdateAuthorRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Email { get; set; }
}