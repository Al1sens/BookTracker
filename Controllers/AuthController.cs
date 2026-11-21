using System.Security.Cryptography;
using System.Text;
using BookTracker.Data;
using BookTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // ШАГ 1: Описываем, какие данные ждём от браузера при входе или регистрации
        public class AuthRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        // ШАГ 2: Регистрация нового пользователя (POST /api/auth/register)
        [HttpPost("register")]
        public async Task<IActionResult> Register(AuthRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Логин и пароль не могут быть пустыми.");
            }

            string cleanUsername = request.Username.Trim();

            // Достаём список всех пользователей из базы и обычным циклом проверяем, не занят ли логин
            var allUsers = await _context.Users.ToListAsync();
            foreach (var existingUser in allUsers)
            {
                if (existingUser.Username == cleanUsername)
                {
                    return BadRequest("Такой логин уже занят. Выбери другой");
                }
            } // <-- ВОТ ЗДЕСЬ закрываем цикл foreach!

            // Создаём нового пользователя и сохраняем хэш его пароля
            var user = new User
            {
                Username = cleanUsername,
                PasswordHash = HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { userId = user.Id, username = user.Username });
        } // <-- ВОТ ЗДЕСЬ закрываем метод Register!

        // ШАГ 3: Вход в аккаунт (POST /api/auth/login)
        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthRequest request)
        {
            string cleanUsername = request.Username.Trim(); // Исправили clearUsername -> cleanUsername
            string inputHash = HashPassword(request.Password);

            // Достаём пользователей и ищем совпадение логина и хэша пароля через обычный цикл
            var allUsers = await _context.Users.ToListAsync();
            User? foundUser = null;

            foreach (var u in allUsers)
            {
                if (u.Username == cleanUsername && u.PasswordHash == inputHash)
                {
                    foundUser = u;
                    break;
                }
            }

            if (foundUser == null)
            {
                return Unauthorized("Неверный логин или пароль");
            }

            return Ok(new { userId = foundUser.Id, username = foundUser.Username });
        }

        // ШАГ 4: Превращаем пароль в криптографический хэш SHA-256
        private static string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}