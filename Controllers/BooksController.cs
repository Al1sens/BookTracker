using BookTracker.Data;
using BookTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookTracker.Controllers
{

    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<Book> GetBooks(
            [FromQuery] string? status,
            [FromQuery] string? search)
        {
            foreach (Book book in _context.Books)
            {
                // Если выбрана вкладка "Прочитанные", а книга не прочитана — пропускаем
                if (status == "read" && book.IsRead == false)
                {
                    continue;
                }

                // Если выбрана вкладка "В планах", а книга уже прочитана — пропускаем
                if (status == "unread" && book.IsRead == true)
                {
                    continue;
                }

                // Если пользователь ввёл слово в поиск — проверяем совпадение
                if (search != null && search != "")
                {
                    string lowerSearch = search.ToLower();
                    string lowerTitle = book.Title.ToLower();
                    string lowerAuthor = book.Autor.ToLower();

                    if(!lowerTitle.Contains(lowerSearch) && !lowerAuthor.Contains(lowerSearch))
                    {
                        continue;
                    }
                }

                yield return book;
            }
        }
        

        [HttpPost]
        public async Task<IActionResult> AddBook([FromBody] Book newBook)
        {
            _context.Books.Add(newBook);
            await _context.SaveChangesAsync();
            return Ok(newBook);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
       
            var book = await _context.Books.FindAsync(id);
            
            if(book == null)
            {
                return NotFound();
            }

            book.IsRead = !book.IsRead;

            book.ReadDate = book.IsRead ? DateTime.Now : null;

            await _context.SaveChangesAsync();

            return Ok(book);


        }

        // ШАГ 2: Статистика чтения в месяц (GET /api/books/stats)
        [HttpGet("stats")]
        public IActionResult GetReadingStats()
        {
            int totalRead = 0;

            HashSet<string> uniqueMonth = new HashSet<string>();

            foreach (Book book in _context.Books)
            {
                if (book.IsRead == true && book.ReadDate != null)
                {
                    totalRead = totalRead + 1;

                    DateTime date = book.ReadDate.Value;
                    uniqueMonth.Add(date.Year + "-" + date.Month);


                }
            }

            if(totalRead == 0)
            {
                return Ok(new { totalRead = 0, averagePerMorth = 0.0 });
            }


            double average = Math.Round((double)totalRead / uniqueMonth.Count, 1);

            return Ok(new
            {
                totalRead = totalRead,
                averagePerMonth = average
            });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if(book == null)
            {
                return NotFound();
            }

            _context.Books.Remove(book);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
