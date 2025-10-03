using Microsoft.EntityFrameworkCore;
using NewsHub.Models;

namespace NewsHub.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<UserCred> UserCreds { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<UserFollow> UserFollows { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<View> Views { get; set; }
        public DbSet<CommentVote> CommentVotes { get; set; }
        public DbSet<TempUser> TempUsers { get; set; }       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //CommentVote value Should be -1 or 1
            modelBuilder.Entity<CommentVote>()
                .ToTable(t => t.HasCheckConstraint("CK_CommentVote_Value", "[Value] = -1 OR [Value] = 1"));

            //Feedback Rating between 1 and 5
            modelBuilder.Entity<Feedback>()
                .ToTable(t => t.HasCheckConstraint("CK_Feedback_Rating", "[Rating] >= 1 AND [Rating] <= 5"));

            //Vote many to many
            modelBuilder.Entity<CommentVote>()
                .HasKey(c => new { c.UserId, c.CommentId });
            modelBuilder.Entity<CommentVote>()
                .HasOne(c => c.User)
                .WithMany(u => u.Votes)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //Views many to many 
            modelBuilder.Entity<View>()
                .HasKey(v => new { v.ArticleId, v.UserId });
            modelBuilder.Entity<View>()
                .HasOne(v => v.User)
                .WithMany(u => u.Views)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            //many to many for view
            modelBuilder.Entity<View>()
                .HasKey(v => new { v.ArticleId, v.UserId });

            modelBuilder.Entity<View>()
                .HasOne(v => v.User)
                .WithMany(u => u.Views)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //many to many for comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //many to many for bookmark
            modelBuilder.Entity<Bookmark>()
                .HasKey(b => new { b.UserId, b.ArticleId });

            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //report
            modelBuilder.Entity<Report>()
                .HasKey(r => new { r.UserId, r.ArticleId });

            modelBuilder.Entity<Report>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reports)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //Report Reason Type Should be one of the enum values
            modelBuilder.Entity<Report>()
                .Property(r => r.Reason)
                .HasConversion<string>();

            //many to many for reaction
            modelBuilder.Entity<Reaction>()
                .Property(r => r.Type)
                .HasConversion<string>();

            modelBuilder.Entity<Reaction>()
                .HasKey(reaction => new { reaction.ArticleId, reaction.UserId });

            modelBuilder.Entity<Reaction>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reactions)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            //many to many self referencing for user follow
            modelBuilder.Entity<UserFollow>()
                .HasKey(uf => new { uf.FollowerId, uf.FolloweeId });

            modelBuilder.Entity<UserFollow>()
                .HasOne(uf => uf.Follower)
                .WithMany(u => u.Followees)
                .HasForeignKey(uf => uf.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserFollow>()
                .HasOne(uf => uf.Followee)
                .WithMany(u => u.Followers)
                .HasForeignKey(uf => uf.FolloweeId)
                .OnDelete(DeleteBehavior.Restrict);

            //turn off cascade delete globally
            // foreach (var fk in modelBuilder.Model.GetEntityTypes()
            //                                      .SelectMany(t => t.GetForeignKeys()))
            // {
            //     fk.DeleteBehavior = DeleteBehavior.Restrict;
            // }

            base.OnModelCreating(modelBuilder);
        }
    }
}
