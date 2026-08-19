using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using BlogApi.Data;
using BlogApi.Models;
using BlogApi.DTOs.Posts;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController: ControllerBase
{
    private readonly AppDbContext _context;

    public PostsController(AppDbContext context)
    {
        _context = context;
    }

    /* GET api/posts */
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetPosts()
    {
        var posts = await _context.Posts
        .Include(p => p.Tags)
        .Include(p => p.Author)
        .Where(p => p.IsPublished)
        .OrderByDescending(p => p.PublishedAt)
        .ToListAsync();

        return posts.Select(ToResponse).ToList();
    }

    /* GET api/posts/{slug} */
    [HttpGet("{slug}")]
    public async Task<ActionResult<PostResponse>> GetPost(string slug)
    {
        var post = await _context.Posts
        .Include(p => p.Tags)
        .Include(p => p.Author)
        .FirstOrDefaultAsync(p => p.Slug == slug);

        if (post == null) return NotFound();

        return ToResponse(post);
    }

    /* POST api/posts */
    [HttpPost]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<ActionResult<PostResponse>> CreatePost(CreatePostRequest request)
    {
        var author = await _context.Authors.FindAsync(request.AuthorId);
        if (author == null) return NotFound();

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

        // Handling tags, if any reuse if not create new
        foreach (var tagName in request.TagNames)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                _context.Tags.Add(tag);
            }

            post.Tags.Add(tag);
        }
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        /* Load post with author data async, since author property not yet loaded upon post creation */
        await _context.Entry(post).Reference(p => p.Author).LoadAsync();

        return CreatedAtAction(nameof (GetPost), new { slug = post.Slug }, ToResponse(post));
    }


    /* PUT /api/posts/{id} */
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<ActionResult<PostResponse>> UpdatePost(int id, UpdatePostRequest request)
    {
        var post = await _context.Posts
        .Include(p => p.Tags)
        .Include(p => p.Author)
        .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null) return NotFound();

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

        foreach (var tagName in request.TagNames)
        {
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);

            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                _context.Tags.Add(tag);
            }

            post.Tags.Add(tag);
        }

        await _context.SaveChangesAsync();

        return ToResponse(post);
    }

    /* DELETE /api/posts/{id} */
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _context.Posts.FindAsync(id);
        
        if (post == null) return NotFound();

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        return NoContent();
    }

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
}