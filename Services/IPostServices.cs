using BlogApi.DTOs.Posts;

namespace BlogApi.Services;

/* Post Service Interface - for Test code integration */
public interface IPostService
{
    Task<List<PostResponse>> GetPublishedPostsAsync();
    Task<PostResponse?> GetPostBySlugAsync(string slug);
    Task<List<PostResponse>> GetPublishedPostsByTagAsync(string tagName);
    Task<PostResponse?> CreatePostAsync(CreatePostRequest request);
    Task<PostResponse?> UpdatePostAsync(int id, UpdatePostRequest request);
    Task<bool> DeletePostAsync(int id);
}