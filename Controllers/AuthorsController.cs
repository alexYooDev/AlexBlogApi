using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

using BlogApi.Models;
using BlogApi.Data;
using BlogApi.DTOs.Authors;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController: ControllerBase
{
    private readonly AppDbContext _context;

    public AuthorsController(AppDbContext context)
    {
        _context = context;
    }

    /* GET /api/authors */
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuthorResponse>>> GetAuthors()
    {
        var authors = await _context.Authors.ToListAsync();

        return authors.Select(ToResponse).ToList();
    }

   /* GET /api/authors/{id} */
    [HttpGet("{id}")]
    public async Task<ActionResult<AuthorResponse>> GetAuthor(int id)
    {
        var author = await _context.Authors.FindAsync(id);

        if (author == null) return NotFound();

        return ToResponse(author);
    }

    /* POST /api/authors */
    [HttpPost]
    public async Task<ActionResult<CreateAuthorResponse>> CreateAuthor(CreateAuthorRequest request)
    {
        var author = new Author
        {
            Name = request.Name,
            Bio = request.Bio,
            Email = request.Email,
            ApiKey = GenerateApiKey()
        };

        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        var response = new CreateAuthorResponse
        {
            Id = author.Id,
            Name = author.Name,
            Bio = author.Bio,
            Email = author.Email,
            ApiKey = author.ApiKey
        };

        return CreatedAtAction(nameof (GetAuthor), new { id = author.Id }, response);
    }

    /* PUT /api/authors/{id} */
    [HttpPut("{id}")]
    public async Task<ActionResult<AuthorResponse>> UpdateAuthor(int id, UpdateAuthorRequest request)
    {
        var author = await _context.Authors.FindAsync(id);

        if (author == null) return NotFound();

        author.Name = request.Name;
        author.Bio = request.Bio;
        author.Email = request.Email;

        await _context.SaveChangesAsync();

        return ToResponse(author);
    }

    /* DELETE /api/authors/{id} */
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        var author = await _context.Authors.FindAsync(id);
        
        if (author == null) return NotFound();

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    private static string GenerateApiKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");
    }

    private AuthorResponse ToResponse(Author author)
    {
        return new AuthorResponse
        {
            Id = author.Id,
            Name = author.Name,
            Bio = author.Bio,
            Email = author.Email
        };
    }
}