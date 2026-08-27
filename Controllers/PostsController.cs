using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using BlogApi.Data;
using BlogApi.Models;
using BlogApi.DTOs.Posts;
using BlogApi.Services;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController: ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    /* GET api/posts */
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostResponse>>> GetPosts()
    {
        var posts = await _postService.GetPublishedPostsAsync();

        return posts;
    }

    /* GET api/posts/{slug} */
    [HttpGet("{slug}")]
    public async Task<ActionResult<PostResponse>> GetPost(string slug)
    {
        var post = await _postService.GetPostBySlugAsync(slug);

        if (post == null) return NotFound();

        return post;
    }

    /* POST api/posts */
    [HttpPost]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<ActionResult<PostResponse>> CreatePost(CreatePostRequest request)
    {
        var created = await _postService.CreatePostAsync(request);

        if (created == null) return BadRequest("Author not found.");

        return CreatedAtAction(nameof (GetPost), new { slug = created.Slug }, created);
    }


    /* PUT /api/posts/{id} */
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<ActionResult<PostResponse>> UpdatePost(int id, UpdatePostRequest request)
    {
        var updated = await _postService.UpdatePostAsync(id, request);

        if (updated == null) return NotFound();

        return updated;
    }

    /* DELETE /api/posts/{id} */
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<IActionResult> DeletePost(int id)
    {
        var deleted = await _postService.DeletePostAsync(id);

        if (!deleted) return NotFound();
        
        return NoContent();
    }
}