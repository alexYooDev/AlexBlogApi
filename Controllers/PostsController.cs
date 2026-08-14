using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApi.Data;
using BlogApi.Models;

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
    public async Task<ActionResult<IEnumerable<Post>>> GetPosts()
    {
        return await _context.Posts
        .Include(p => p.Tags)
        .Where(p => p.IsPublished)
        .OrderByDescending(p => p.PublishedAt)
        .ToListAsync();
    }

    /* GET api/posts/{slug} */
    [HttpGet]
    public async Task<ActionResult<Post>> GetPost(string slug)
    {
        var post = await _context.Posts
        .Include(p => p.Tags)
        .Include(p => p.Author)
        .FirstOrDefaultAsync(p => p.Slug == slug);

        if (post == null) return NotFound();

        return post;
    }

    /* POST api/posts */
    [HttpPost]
    public async Task<ActionResult<Post>> CreatePost(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof (GetPost), new { slug = post.Slug }, post);
    }
}