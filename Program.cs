using BookTracker.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ШАГ 1: Регистрируем контроллеры API
builder.Services.AddControllers();

// ШАГ 2: Читаем строку подключения из appsettings.json (локально) или переменных окружения (на Render)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ШАГ 3: Подключаем базу данных PostgreSQL вместо SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// ШАГ 4: Настраиваем конвейер обработки запросов (Middleware)
app.UseHttpsRedirection();

app.UseDefaultFiles();

app.UseStaticFiles();

app.MapControllers();

// ШАГ 5: Автоматически накатываем миграции на облачную базу при запуске сервера
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();