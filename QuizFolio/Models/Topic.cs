namespace QuizFolio.Models
{
    public class Topic
    {
        public int Id { get; set; }
        public string TopicName {get; set;}
        public ICollection<Template> Templates { get; set; }
    }
}
