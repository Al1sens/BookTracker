# Babylonian Library (BookTracker)

A full-stack web application for tracking reading progress, taking margin notes, and visualizing a personal book collection as interactive wooden bookshelves.

**Live Demo:** [booktracker-azyz.onrender.com](https://booktracker-azyz.onrender.com)

## Tech Stack
- **Backend:** C#, .NET 10, ASP.NET Core Web API (Controllers)
- **Database & ORM:** PostgreSQL, Entity Framework Core (Code First, Migrations)
- **Frontend:** HTML5, CSS3, Vanilla JavaScript (Fetch API, 3D CSS animations, responsive layout)
- **Deployment:** Docker, Render.com, CI/CD via GitHub

## Key Features
- **Keeper's Dashboard:** User registration and authentication, tracking read/unread books, calculating average monthly reading speed, and overall archive completion percentage.
- **Search & Filtering:** Instant filtering by title, author, genre, and reading status (`All`, `Unread`, `Read`).
- **Infinite Reading Hall:** Two display modes — a continuous flow of books (`General Hall`) and automatic shelf grouping with brass plaques (`By Genre`, plus a general shelf for uncategorized books).
- **Interactive Grimoire:** 3D book-opening animation with persistent margin notes and quotes saved directly to the database via debounced `PATCH` requests.

## Project Structure
- `Models/` — Entity definitions (`Book`, `User`)
- `Data/` — `AppDbContext` and EF Core database migrations
- `Controllers/` — RESTful API endpoints (`BooksController`, `AuthController`)
- `wwwroot/` — Static frontend assets (`index.html`, `library.html`)
