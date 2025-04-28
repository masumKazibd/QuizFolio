namespace QuizFolio.Models
{
    public class Template
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public string CreatorId { get; set; }
        public Users Creator { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<FormResponse> FormResponses { get; set; }
    }
}
