using Microsoft.EntityFrameworkCore;
using BlogApi.DTOs.Authors;
using BlogApi.Data;
using BlogApi.Models;

using System.Security.Cryptography;
using SQLitePCL;

namespace BlogApi.Services;

public class AuthorService : IAuthorService
{
    private readonly AppDbContext _context;


    public AuthorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AuthorResponse>> GetAuthorsAsync()
    {
        var authors = await _context.Authors.ToListAsync();

        return authors.Select(ToResponse).ToList();
    }

    public async Task<AuthorResponse?> GetAuthorAsync(int id)
    {
        var author = await _context.Authors.FindAsync(id);

        return author == null ? null : ToResponse(author);
    }

    public async Task<CreateAuthorResponse?> CreateAuthorAsync(CreateAuthorRequest request)
    {

        var rawApiKey = GenerateApiKey();

        var author = new Author
        {
            Name = request.Name,
            Email = request.Email,
            Bio = request.Bio,
            ApiKey = ApiKeyHasher.Hash(rawApiKey) // Hash the raw api string 
        };

        _context.Authors.Add(author);
        await _context.SaveChangesAsync();

        return new CreateAuthorResponse
        {
            Id = author.Id,
            Name = author.Name,
            Bio = author.Bio,
            Email = author.Email,
            ApiKey = rawApiKey
        };
    }

    public async Task<AuthorResponse?> UpdateAuthorAsync(int id, UpdateAuthorRequest request)
    {
        var author = await _context.Authors.FindAsync(id);
        
        if (author == null) return null;

        author.Name = request.Name;
        author.Email = request.Email;
        author.Bio = request.Bio;

        await _context.SaveChangesAsync();

        return ToResponse(author);
    }

    public async Task<bool> DeleteAuthorAsync(int id)
    {
        var author = await _context.Authors.FindAsync(id);

        if (author == null) return false;
        
        _context.Authors.Remove(author);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<CreateAuthorResponse?> RegenerateApiKeyAsync(int id)
    {
        var author = await _context.Authors.FindAsync(id);
        if (author == null) return null;

        var rawApiKey = GenerateApiKey();
        author.ApiKey = ApiKeyHasher.Hash(rawApiKey);
        await _context.SaveChangesAsync();

        return new CreateAuthorResponse
        {
            Id = author.Id,
            Name = author.Name,
            Bio = author.Bio,
            Email = author.Email,
            ApiKey = rawApiKey
        };
    }

    /* private helper functions */

    private static AuthorResponse ToResponse(Author author)
    {
        return new AuthorResponse
        {
            Id = author.Id,
            Name = author.Name,
            Bio = author.Bio,
            Email = author.Email
        };
    }

    private static string GenerateApiKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");
    }
}