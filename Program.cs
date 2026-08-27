using BlogApi.Data;
using BlogApi.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi;
using BlogApi.Services;

var builder = WebApplication.CreateBuilder(args);

/* CORS Policy */
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClientAccess", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=blog.db"));

builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();

/* Add Controllers */
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.OpenApiSecurityScheme 
    {
        Name = "X-Api-Key",
        Type = Microsoft.OpenApi.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "API Key Authentication. Enter your key in the header as: X-Api-Key: {your-key}"
    });
    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("ApiKey", document)] = new List<string>()
    });
});

builder.Services.AddAuthentication("ApiKey")
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", options => {});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowClientAccess");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
