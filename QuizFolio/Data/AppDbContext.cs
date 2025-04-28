using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuizFolio.Models;

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
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<Template>(entity =>
            {
                entity.HasOne(t => t.Creator)
                      .WithMany(u => u.Templates)
                      .HasForeignKey(t => t.CreatorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Question>(entity =>
            {
                entity.HasOne(q => q.Template)
                      .WithMany(t => t.Questions)
                      .HasForeignKey(q => q.TemplateId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            //builder.Entity<FormResponse>(entity =>
            //{
            //    entity.HasOne(f => f.Template)
            //          .WithMany(t => t.FormResponses)
            //          .HasForeignKey(f => f.TemplateId)
            //          .OnDelete(DeleteBehavior.Restrict);

            //    entity.HasOne(f => f.Respondent)
            //          .WithMany(u => u.FormResponses)
            //          .HasForeignKey(f => f.RespondentId)
            //          .OnDelete(DeleteBehavior.SetNull);
            //});


            builder.Entity<QuestionOption>(entity =>
            {
                entity.HasOne(qo => qo.Question)
                      .WithMany(q => q.Options)
                      .HasForeignKey(qo => qo.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


        }
    }
}