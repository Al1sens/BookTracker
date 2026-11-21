namespace BookTracker.Models
{
    public class Book
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadDate { get; set; }
        public int UserId { get; set; }

        public string? Genre { get; set; }
    }
}
