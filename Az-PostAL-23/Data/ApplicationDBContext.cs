using Az_PostAL_23.Models;
using Microsoft.EntityFrameworkCore;

namespace Az_PostAL_23.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
            
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Transaction> Transaction { get; set; }

    }
}
