using Microsoft.EntityFrameworkCore;
using LaundryManagement.Models.Entities;

namespace LaundryManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> users { get; set; } = null!;
        public DbSet<Jasa> Jasas { get; set; } = null!;
        public DbSet<PricelistJasa> PricelistJasas { get; set; } = null!;
    }
}