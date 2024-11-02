using Microsoft.EntityFrameworkCore;

namespace TechNest.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
                
        }
    }
}
