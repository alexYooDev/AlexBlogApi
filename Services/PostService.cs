using Microsoft.EntityFrameworkCore;
using BlogApi.Data;
using BlogApi.Models;
using BlogApi.DTOs.Posts;

namespace BlogApi.Services;

/* Implements Post interface */
public class PostService : IPostService
{
    private readonly AppDbContext _context;
    public PostService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PostResponse>> GetPublishedPostsAsync()
    {
        var posts = await _context.Posts
        .Include(p => p.Tags)
        .Include(p => p.Author)
        .Where(p => p.IsPublished)
        .OrderByDescending(p => p.PublishedAt)
        .ToListAsync();

        return posts.Select(ToResponse).ToList();
    }

    public async Task<PostResponse?> GetPostBySlugAsync(string slug)
    {
        var post = await _context.Posts
            .Include(p => p.Tags)
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Slug == slug);

            return post == null ? null : ToResponse(post);
    }

    public async Task<List<PostResponse>> GetPublishedPostsByTagAsync(string tagName)
    {
        var tag = await _context.Tags
            .Include(t => t.Posts)
                .ThenInclude(p => p.Author)
            .Include(t => t.Posts)
                .ThenInclude(p => p.Tags)
            .FirstOrDefaultAsync(t => t.Name == tagName);

        if (tag == null) return new List<PostResponse>();

        return tag.Posts.Where(p => p.IsPublished).Select(ToResponse).ToList();
    }

    public async Task<PostResponse?> CreatePostAsync(CreatePostRequest request)
    {
        var author = await _context.Authors.FindAsync(request.AuthorId);

        if (author == null) return null;

        var post = new Post
        {
            Title = request.Title,
            Slug = request.Slug,
            Content = request.Content,
            Summary = request.Summary,
            IsPublished = request.IsPublished,
            PublishedAt = request.IsPublished ? DateTime.UtcNow : null,
            AuthorId = request.AuthorId
        };
        
        await AttachTagsAsync(post, request.TagNames);

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        /* Load post with author data async, since author property not yet loaded upon post creation */
        await _context.Entry(post).Reference(p => p.Author).LoadAsync();

        return ToResponse(post);
    }

    public async Task<PostResponse?> UpdatePostAsync(int id, UpdatePostRequest request)
    {
        var post = await _context.Posts
            .Include(p => p.Tags)
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        if (post == null) return null;
        
        post.Title = request.Title;
        post.Slug = request.Slug;
        post.Content = request.Content;
        post.Summary = request.Summary;

        if (request.IsPublished && !post.IsPublished)
        {
            post.PublishedAt = DateTime.UtcNow;
        }
        post.IsPublished = request.IsPublished;

        post.Tags.Clear();

        await AttachTagsAsync(post, request.TagNames);
        await _context.SaveChangesAsync();

        return ToResponse(post);
    }

    public async Task<bool> DeletePostAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null) return false;

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
         
        return true;
    }

    /* Private Scope Helper Functions */

    private static PostResponse ToResponse(Post post)
    {
        return new PostResponse
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Content = post.Content,
            Summary = post.Summary,
            CreatedAt = post.CreatedAt,
            PublishedAt = post.PublishedAt,
            IsPublished = post.IsPublished,
            AuthorName = post.Author.Name,
            Tags = post.Tags.Select(t => t.Name).ToList()
        };
    }

    private async Task AttachTagsAsync(Post post, List<string> tagNames)
    {
        foreach (var tagName in tagNames)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                _context.Tags.Add(tag);
            }
            post.Tags.Add(tag);
        }
    }
}