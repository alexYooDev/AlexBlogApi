using BlogApi.DTOs.Authors;

namespace BlogApi.Services;

public interface IAuthorService
{
    Task<List<AuthorResponse>> GetAuthorsAsync();
    Task<AuthorResponse?> GetAuthorAsync(int id);
    Task<CreateAuthorResponse?> CreateAuthorAsync(CreateAuthorRequest request);
    Task<AuthorResponse?> UpdateAuthorAsync(int id, UpdateAuthorRequest request);
    Task<bool> DeleteAuthorAsync(int id);
    Task<CreateAuthorResponse?> RegenerateApiKeyAsync(int id); 
}