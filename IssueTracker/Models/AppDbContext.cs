using System;
using System.Linq;
using IssueTracker.Models.CombModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IssueTracker.Models
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {

        }

        //public DbSet<User> Users { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ScreenShots> ScreenShots { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<IssueHistory> IssueHistory { get; set; }
        public DbSet<ProjectIssue> ProjectIssues { get; set; }
        public DbSet<UserProjects> UserProjects { get; set; }
        public DbSet<ProjectHistory> ProjectHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

        }
    }


}
