using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using BlogApi.Data;
using BlogApi.Models;
using BlogApi.DTOs.Tags;
using BlogApi.DTOs.Posts;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController: ControllerBase
{
    private readonly AppDbContext _context;
    
    public TagsController(AppDbContext context)
    {
        _context = context;
    }

    /* GET /api/tags */
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagResponse>>> GetTags()
    {
        var tags = await _context.Tags
            .Include(t => t.Posts)
            .ToListAsync();

        return tags.Select(t => new TagResponse
        {
            Id = t.Id,
            Name = t.Name,
            PostCount = t.Posts.Count(p => p.IsPublished)
        }).ToList();
    }

    /* GET /api/tags/{name}/posts */
    [HttpGet("{name}")]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetPostsByTag(string name)
    {
        var tag = await _context.Tags
        .Include(t => t.Posts)
            .ThenInclude(p => p.Author)
        .Include(t => t.Posts)
            .ThenInclude(p => p.Tags)
        .FirstOrDefaultAsync(t => t.Name == name);

        if (tag == null) return NotFound();

        var posts = tag.Posts.Where(p => p.IsPublished);

        return posts.Select(p => new PostResponse
        {
            Id = p.Id,
            Title = p.Title,
            Slug = p.Slug,
            Content = p.Content,
            Summary = p.Summary,
            CreatedAt = p.CreatedAt,
            PublishedAt = p.PublishedAt,
            IsPublished = p.IsPublished,
            AuthorName = p.Author.Name,
            Tags = p.Tags.Select(t => t.Name).ToList()
        }).ToList();
    }
}

