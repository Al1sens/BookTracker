
using BookTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Book> Books { get; set; }

        public DbSet<User> Users { get; set; }
       
    }
}
