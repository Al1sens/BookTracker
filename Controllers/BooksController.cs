using BookTracker.Data;
using BookTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        // ШАГ 1: Достаём числовой Id пользователя из заголовка X-User-Id
        private int GetCurrentUserId()
        {
            string headerValue = Request.Headers["X-User-Id"].ToString();
            int.TryParse(headerValue, out int userId);
            return userId;
        }

        // ШАГ 2: Отдаём только книги вошедшего пользователя
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks(string? status, string? search)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized("Сначала войдите в аккаунт.");

            var allBooks = await _context.Books.ToListAsync();
            var result = new List<Book>();

            foreach (var book in allBooks)
            {
                if (book.UserId != userId)
                    continue;

                if (status == "read" && !book.IsRead)
                    continue;

                if (status == "unread" && book.IsRead)
                    continue;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    bool matchTitle = book.Title.Contains(search, StringComparison.OrdinalIgnoreCase);
                    bool matchAuthor = book.Autor.Contains(search, StringComparison.OrdinalIgnoreCase);
                    if (!matchTitle && !matchAuthor)
                        continue;
                }

                result.Add(book);
            }

            return Ok(result);
        }

        // ШАГ 3: Статистика только по книгам текущего пользователя
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetStats()
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var allBooks = await _context.Books.ToListAsync();
            int totalRead = 0;
            var activeMonths = new HashSet<string>();

            foreach (var book in allBooks)
            {
                if (book.UserId == userId && book.IsRead)
                {
                    totalRead++;
                    if (book.ReadDate.HasValue)
                    {
                        activeMonths.Add($"{book.ReadDate.Value.Year}-{book.ReadDate.Value.Month}");
                    }
                }
            }

            int monthsCount = activeMonths.Count == 0 ? 1 : activeMonths.Count;
            double averagePerMonth = Math.Round((double)totalRead / monthsCount, 1);

            return Ok(new { totalRead, averagePerMonth });
        }

        // ШАГ 4: Привязываем новую книгу к текущему пользователю
        [HttpPost]
        public async Task<ActionResult<Book>> AddBook(Book book)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            book.UserId = userId;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return Ok(book);
        }

        // ШАГ 5: Меняем статус только у своей книги
        [HttpPut("{id}")]
        public async Task<IActionResult> ToggleRead(int id)
        {
            int userId = GetCurrentUserId();
            var book = await _context.Books.FindAsync(id);

            if (book == null || book.UserId != userId)
                return NotFound();

            book.IsRead = !book.IsRead;
            book.ReadDate = book.IsRead ? DateTime.UtcNow : null;

            await _context.SaveChangesAsync();
            return Ok(book);
        }

        // ШАГ 6: Удаляем только свою книгу
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            int userId = GetCurrentUserId();
            var book = await _context.Books.FindAsync(id);

            if (book == null || book.UserId != userId)
                return NotFound();

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}