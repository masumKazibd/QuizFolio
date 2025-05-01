namespace QuizFolio.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        public int TemplateId { get; set; }
        public Template Template { get; set; }

        public string UserId { get; set; }
        public Users User { get; set; }
    }
}
