using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuizFolio.Models;
using System.Reflection.Emit;

namespace QuizFolio.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<FormResponse> FormResponses { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Template>(entity =>
            {
                entity.HasOne(t => t.Creator)
                      .WithMany(u => u.Templates)
                      .HasForeignKey(t => t.CreatorId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.Topic)
                      .WithMany(topic => topic.Templates)
                      .HasForeignKey(t => t.TopicId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Comment>(entity =>
            {
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Comments)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Like>(entity =>
            {
                entity.HasOne(l => l.User)
                      .WithMany(u => u.Likes)
                      .HasForeignKey(l => l.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<Question>(entity =>
            {
                entity.HasOne(q => q.Template)
                      .WithMany(t => t.Questions)
                      .HasForeignKey(q => q.TemplateId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<QuestionOption>(entity =>
            {
                entity.HasOne(qo => qo.Question)
                      .WithMany(q => q.Options)
                      .HasForeignKey(qo => qo.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<Topic>().HasData(
            new Topic { Id = 1, TopicName = "Education" },
            new Topic { Id = 2, TopicName = "Quiz" },
            new Topic { Id = 3, TopicName = "Other" }
            );
        }
    }
}