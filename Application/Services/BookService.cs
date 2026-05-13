using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using lab11.Application.DTOs;
using lab11.Domain.Models;
using lab11.Infrastructure;

namespace lab11.Application.Services
{
    public class BookService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<BookService> _logger;
        private readonly IMemoryCache _cache;

        private const string BOOKS_CACHE_KEY = "books_cache";

        public BookService(
            AppDbContext context,
            IMapper mapper,
            ILogger<BookService> logger,
            IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _cache = cache;
        }

        public List<BookDto> GetAllBooks()
        {
            if (_cache.TryGetValue(BOOKS_CACHE_KEY, out List<BookDto> cachedBooks))
            {
                _logger.LogInformation("Cache hit: books loaded from cache");

                return cachedBooks;
            }

            _logger.LogInformation("Cache miss: books loaded from database");

            var books = _context.Books
                .Include(b => b.Author)
                .ToList();

            var mappedBooks = _mapper.Map<List<BookDto>>(books);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(BOOKS_CACHE_KEY, mappedBooks, cacheOptions);

            _logger.LogInformation("Books saved to cache. Count: {Count}", mappedBooks.Count);

            return mappedBooks;
        }

        public BookDto? GetBookById(int id)
        {
            _logger.LogInformation("Getting book by id: {Id}", id);

            var book = _context.Books
                .Include(b => b.Author)
                .FirstOrDefault(b => b.Id == id);

            if (book == null)
            {
                _logger.LogWarning("Book with id {Id} was not found", id);
                return null;
            }

            return _mapper.Map<BookDto>(book);
        }

        public List<BookDto> SearchBooks(string title)
        {
            _logger.LogInformation("Searching books by title: {Title}", title);

            var books = _context.Books
                .Include(b => b.Author)
                .Where(b => b.Title.Contains(title))
                .ToList();

            _logger.LogInformation("Search completed. Found books: {Count}", books.Count);

            return _mapper.Map<List<BookDto>>(books);
        }

        public BookDto AddBook(CreateBookDto dto)
        {
            _logger.LogInformation("Creating book: {Title}, AuthorId: {AuthorId}", dto.Title, dto.AuthorId);

            var book = new Book
            {
                Title = dto.Title,
                AuthorId = dto.AuthorId
            };

            _context.Books.Add(book);
            _context.SaveChanges();

            _cache.Remove(BOOKS_CACHE_KEY);
            _logger.LogInformation("Cache cleared after creating book");

            var createdBook = _context.Books
                .Include(b => b.Author)
                .FirstOrDefault(b => b.Id == book.Id);

            _logger.LogInformation("Book created successfully. Id: {Id}", book.Id);

            return _mapper.Map<BookDto>(createdBook);
        }

        public BookDto? UpdateBook(int id, UpdateBookDto dto)
        {
            _logger.LogInformation("Updating book id: {Id}", id);

            var book = _context.Books.Find(id);

            if (book == null)
            {
                _logger.LogWarning("Book with id {Id} was not found for update", id);
                return null;
            }

            book.Title = dto.Title;
            book.AuthorId = dto.AuthorId;

            _context.SaveChanges();

            _cache.Remove(BOOKS_CACHE_KEY);
            _logger.LogInformation("Cache cleared after updating book id: {Id}", id);

            var updatedBook = _context.Books
                .Include(b => b.Author)
                .FirstOrDefault(b => b.Id == id);

            _logger.LogInformation("Book updated successfully. Id: {Id}", id);

            return _mapper.Map<BookDto>(updatedBook);
        }

        public bool DeleteBook(int id)
        {
            _logger.LogInformation("Deleting book id: {Id}", id);

            var book = _context.Books.Find(id);

            if (book == null)
            {
                _logger.LogWarning("Book with id {Id} was not found for delete", id);
                return false;
            }

            _context.Books.Remove(book);
            _context.SaveChanges();

            _cache.Remove(BOOKS_CACHE_KEY);
            _logger.LogInformation("Cache cleared after deleting book id: {Id}", id);

            return true;
        }
    }
}