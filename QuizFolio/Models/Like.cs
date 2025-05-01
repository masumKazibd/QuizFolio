namespace QuizFolio.Models
{
    public class Like
    {
        public int Id { get; set; }

        // Foreign key to the template
        public int TemplateId { get; set; }
        public Template Template { get; set; }

        // Foreign key to the user who liked the template
        public string UserId { get; set; }
        public Users User { get; set; }
    }
}
