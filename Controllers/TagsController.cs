using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using BlogApi.Data;
using BlogApi.Models;
using BlogApi.DTOs.Tags;
using BlogApi.DTOs.Posts;
using BlogApi.Services;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController: ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPostService _postService;

    public TagsController(AppDbContext context, IPostService postService)
    {
        _context = context;
        _postService = postService;
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

    /* GET /api/tags/{name} */
    [HttpGet("{name}")]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetPostsByTag(string name)
    {
        var posts = await _postService.GetPublishedPostsByTagAsync(name);

        return posts;
    }
}

