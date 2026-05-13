using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using lab11.Infrastructure;
using lab11.Application.Services;
using lab11.Application.Mapping;
using lab11.Middleware;
using lab11.Domain.Models;
using lab11.Filters;
using lab11.Auth;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        "Server=(localdb)\\MSSQLLocalDB;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;"));

// Services
builder.Services.AddScoped<BookService>();

// Cache
builder.Services.AddMemoryCache();

// Filter
builder.Services.AddScoped<LogActionFilter>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Authentication
builder.Services.AddAuthentication("TestAuth")
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
        "TestAuth", null);

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanEditBooksPolicy", policy =>
    {
        policy.RequireClaim("CanEditBooks", "true");
    });
});

// Controllers + Filter
builder.Services.AddControllers(options =>
{
    options.Filters.Add<LogActionFilter>();
});

// Swagger documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Library Web API",
        Version = "v1",
        Description = "API for managing books in a library system"
    });
});

var app = builder.Build();

// Exception middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Тестові дані
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!context.Authors.Any())
    {
        var king = new Author
        {
            Name = "Stephen King"
        };

        context.Authors.Add(king);
        context.SaveChanges();

        context.Books.AddRange(
            new Book
            {
                Title = "It",
                AuthorId = king.Id
            },
            new Book
            {
                Title = "The Shining",
                AuthorId = king.Id
            },
            new Book
            {
                Title = "Pet Sematary",
                AuthorId = king.Id
            }
        );

        context.SaveChanges();
    }
}

app.Run();