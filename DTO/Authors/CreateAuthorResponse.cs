using BlogApi.DTOs.Authors;

public class CreateAuthorResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Email { get; set; }
    public string ApiKey { get; set; } = string.Empty;
}