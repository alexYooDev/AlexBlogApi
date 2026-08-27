using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

using BlogApi.Models;
using BlogApi.Data;
using BlogApi.DTOs.Authors;
using Microsoft.AspNetCore.Authorization;
using BlogApi.Services;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController: ControllerBase
{
    private readonly IAuthorService _authorService;

    public AuthorsController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    /* GET /api/authors */
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuthorResponse>>> GetAuthors()
    {
        var authors = await _authorService.GetAuthorsAsync();

        return authors;
    }

   /* GET /api/authors/{id} */
    [HttpGet("{id}")]
    public async Task<ActionResult<AuthorResponse>> GetAuthor(int id)
    {
        var author = await _authorService.GetAuthorAsync(id);

        if (author == null) return NotFound();

        return author;
    }

    /* POST /api/authors */
    [HttpPost]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<ActionResult<CreateAuthorResponse>> CreateAuthor(CreateAuthorRequest request)
    {
        var created = await _authorService.CreateAuthorAsync(request);

        if (created == null ) return BadRequest("Creation of author failed.");

        return CreatedAtAction(nameof (GetAuthor), new { id = created.Id }, created);
    }

    /* POST /api/authors/{id}/regenerate-key */
    [HttpPost("{id}/regenerate-key")]
    public async Task<ActionResult<CreateAuthorResponse>> RegenerateApiKey(int id)
    {
        var regenerated = await _authorService.RegenerateApiKeyAsync(id);

        if (regenerated == null) return NotFound();
        
        return regenerated;
    }

    /* PUT /api/authors/{id} */
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<ActionResult<AuthorResponse>> UpdateAuthor(int id, UpdateAuthorRequest request)
    {
        var updated = await _authorService.UpdateAuthorAsync(id, request);

        if (updated == null) return NotFound();

        return updated;
    }

    /* DELETE /api/authors/{id} */
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = "ApiKey")]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        var deleted = await _authorService.DeleteAuthorAsync(id);
        
        if (!deleted) return NotFound();

        return NoContent();
    }
    
}