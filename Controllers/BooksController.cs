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

        // ШАГ 2: Отдаём книги пользователя с фильтрацией по статусу, поиску и жанру
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks(
            [FromQuery] string? status = null,
            [FromQuery] string? search = null,
            [FromQuery] string? genre = null)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            var query = _context.Books.Where(b => b.UserId == userId);

            if (status == "read")
                query = query.Where(b => b.IsRead);
            else if (status == "unread")
                query = query.Where(b => !b.IsRead);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(s) || b.Autor.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                if (genre == "none")
                    query = query.Where(b => string.IsNullOrEmpty(b.Genre));
                else
                    query = query.Where(b => b.Genre == genre);
            }

            return await query.OrderByDescending(b => b.Id).ToListAsync();
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

        // ШАГ 4: Привязываем новую книгу и её жанр к текущему пользователю
        [HttpPost]
        public async Task<ActionResult<Book>> AddBook(Book book)
        {
            int userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized();

            book.UserId = userId;
            // Если поле жанра оставили пустым — записываем в базу null
            book.Genre = string.IsNullOrWhiteSpace(book.Genre) ? null : book.Genre.Trim();

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

        [HttpPatch("{id}/notes")]
        public async Task<IActionResult> UpdateNotes(int id, [FromBody] string? notes)
        {
            int usedId = GetCurrentUserId();
            var book = await _context.Books.FindAsync(id);

            if (book == null || book.UserId != usedId)
                return NotFound();

            book.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes;
            await _context.SaveChangesAsync();

            return Ok(new { notes = book.Notes });
        }
    }
}