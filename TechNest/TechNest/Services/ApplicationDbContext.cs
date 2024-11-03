using Microsoft.EntityFrameworkCore;
using TechNest.Models;

namespace TechNest.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
                
        }

        public DbSet<Product> Products { get; set; }
    }
}
