using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<long>, long>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Theatre> Theatres { get; set; }
        public DbSet<Screening> Screenings { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Seat> Seats { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure User entity
            builder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasIndex(u => u.UserName).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();

                // Configure enum conversion
                entity.Property(u => u.Role)
                    .HasConversion<string>()
                    .HasMaxLength(50);
            });

            // Configure Movie entity
            builder.Entity<Movie>(entity =>
            {
                entity.ToTable("movies");

                // Configure enum conversions
                entity.Property(m => m.Genre)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.Property(m => m.Rating)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                // Configure text columns
                entity.Property(m => m.PosterImageUrl)
                    .HasColumnType("TEXT");

                entity.Property(m => m.TrailerUrl)
                    .HasColumnType("TEXT");

                entity.Property(m => m.Description)
                    .HasColumnType("TEXT");
            });

            // Configure Theatre entity
            builder.Entity<Theatre>(entity =>
            {
                entity.ToTable("theatres");

                // Configure text columns
                entity.Property(t => t.ImageUrl)
                    .HasColumnType("TEXT");
            });

            // Configure Screening entity
            builder.Entity<Screening>(entity =>
            {
                entity.ToTable("screenings");

                // Configure enum conversion
                entity.Property(s => s.Format)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                // Configure relationships
                entity.HasOne(s => s.Movie)
                    .WithMany(m => m.Screenings)
                    .HasForeignKey(s => s.MovieId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Theatre)
                    .WithMany(t => t.Screenings)
                    .HasForeignKey(s => s.TheatreId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Seat entity
            builder.Entity<Seat>(entity =>
            {
                entity.ToTable("seats");

                // Configure enum conversion
                entity.Property(s => s.SeatType)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                // Configure relationship
                entity.HasOne(s => s.Theatre)
                    .WithMany(t => t.Seats)
                    .HasForeignKey(s => s.TheatreId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Booking entity
            builder.Entity<Booking>(entity =>
            {
                entity.ToTable("bookings");

                // Configure enum conversion
                entity.Property(b => b.PaymentStatus)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                // Configure relationships
                entity.HasOne(b => b.User)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(b => b.Screening)
                    .WithMany(s => s.Bookings)
                    .HasForeignKey(b => b.ScreeningId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure BookedSeats as TEXT to handle comma-separated values
                entity.Property(b => b.BookedSeats)
                    .HasColumnType("TEXT");
            });

            // Configure Identity tables to use custom names if needed
            builder.Entity<IdentityRole<long>>().ToTable("roles");
            builder.Entity<IdentityUserRole<long>>().ToTable("user_roles");
            builder.Entity<IdentityUserClaim<long>>().ToTable("user_claims");
            builder.Entity<IdentityUserLogin<long>>().ToTable("user_logins");
            builder.Entity<IdentityUserToken<long>>().ToTable("user_tokens");
            builder.Entity<IdentityRoleClaim<long>>().ToTable("role_claims");
        }
    }
}