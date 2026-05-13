using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using lab11.Application.DTOs;
using lab11.Application.Services;

namespace lab11.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly BookService _bookService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(BookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            _logger.LogInformation("GET /api/Books called");

            var userRole = User.Claims.FirstOrDefault(c => c.Type.Contains("role"))?.Value;
            _logger.LogInformation("Current user role: {Role}", userRole ?? "Anonymous");

            var books = _bookService.GetAllBooks();

            _logger.LogInformation("Returned {Count} books", books.Count);

            return Ok(books);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            _logger.LogInformation("GET /api/Books/{Id} called", id);

            if (id <= 0)
            {
                _logger.LogWarning("Invalid book id: {Id}", id);
                return BadRequest("Id must be greater than 0");
            }

            var book = _bookService.GetBookById(id);

            if (book == null)
            {
                _logger.LogWarning("Book with id {Id} not found", id);
                return NotFound("Book not found");
            }

            return Ok(book);
        }

        [HttpGet("search")]
        public IActionResult Search([FromQuery] string title)
        {
            _logger.LogInformation("Search books by title: {Title}", title);

            if (string.IsNullOrWhiteSpace(title))
            {
                _logger.LogWarning("Search title is empty");
                return BadRequest("Title query parameter is required");
            }

            var books = _bookService.SearchBooks(title);

            _logger.LogInformation("Search returned {Count} books", books.Count);

            return Ok(books);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public IActionResult CreateBook(CreateBookDto dto)
        {
            _logger.LogInformation("POST /api/Books called. Title: {Title}", dto.Title);

            var role = User.Claims.FirstOrDefault(c => c.Type.Contains("role"))?.Value;
            _logger.LogInformation("Authorized user role for create: {Role}", role);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state while creating book");
                return BadRequest(ModelState);
            }

            if (dto.Title == "error")
            {
                _logger.LogError("Test exception was triggered");
                throw new Exception("Test exception");
            }

            var createdBook = _bookService.AddBook(dto);

            _logger.LogInformation("Book created successfully. Id: {Id}", createdBook.Id);

            return CreatedAtAction(nameof(GetById),
                new { id = createdBook.Id }, createdBook);
        }

        [Authorize(Policy = "CanEditBooksPolicy")]
        [HttpPut("{id}")]
        public IActionResult UpdateBook(int id, UpdateBookDto dto)
        {
            _logger.LogInformation("PUT /api/Books/{Id} called", id);

            var canEdit = User.Claims.FirstOrDefault(c => c.Type == "CanEditBooks")?.Value;
            _logger.LogInformation("User CanEditBooks claim: {CanEdit}", canEdit);

            if (id <= 0)
            {
                _logger.LogWarning("Invalid book id for update: {Id}", id);
                return BadRequest("Id must be greater than 0");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state while updating book id: {Id}", id);
                return BadRequest(ModelState);
            }

            var updatedBook = _bookService.UpdateBook(id, dto);

            if (updatedBook == null)
            {
                _logger.LogWarning("Book with id {Id} not found for update", id);
                return NotFound("Book not found");
            }

            _logger.LogInformation("Book with id {Id} updated successfully", id);

            return Ok(updatedBook);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteBook(int id)
        {
            _logger.LogInformation("DELETE /api/Books/{Id} called", id);

            if (id <= 0)
            {
                _logger.LogWarning("Invalid book id for delete: {Id}", id);
                return BadRequest("Id must be greater than 0");
            }

            var result = _bookService.DeleteBook(id);

            if (!result)
            {
                _logger.LogWarning("Book with id {Id} not found for delete", id);
                return NotFound("Book not found");
            }

            _logger.LogInformation("Book with id {Id} deleted successfully", id);

            return Ok("Book deleted successfully");
        }
    }
}