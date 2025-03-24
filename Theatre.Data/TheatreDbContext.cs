using Microsoft.EntityFrameworkCore;
using Theatre.Core.Models;
namespace Theatre.Data
{
    public class TheatreDbContext : DbContext
    {
        public DbSet<Spectacles> Spectacles { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AdminKey> AdminKeys { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<SpectacleGenre> SpectacleGenres { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=Users;Username=postgres;Password=Mailgame5250580;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SpectacleGenre>()
                .HasKey(sg => new { sg.SpectacleId, sg.GenreId });
            modelBuilder.Entity<SpectacleGenre>()
                .HasOne(sg => sg.Spectacle)
                .WithMany(s => s.Genres)
                .HasForeignKey(sg => sg.GenreId);

            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Spectacle)
                .WithMany(s => s.Seats)
                .HasForeignKey(s => s.SpectacleId);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Seat)
                .WithMany()
                .HasForeignKey(t => t.SeatId);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId);
        }
        //public bool Authenticate(string login, string password)
        //{
        //    using (var context = new TheatreDbContext())
        //    {
        //        var user = context.Users.FirstOrDefault(u => u.Login == login && u.Password ==
        //        userQuaries.HashPassword(password));
        //        if (user != null)
        //        {
        //            currentUser.Id = user.Id;
        //            currentUser.Login = user.Login;
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }
}
