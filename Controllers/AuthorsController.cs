using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<AuthorResponse>> CreateAuthor(CreateAuthorRequest request)
    {
        var author = new Author
        {
            Name = request.Name,
            Bio = request.Bio,
            Email = request.Email
        };

        _context.Authors.Add(author);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof (GetAuthor), new { id = author.Id }, ToResponse(author));
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