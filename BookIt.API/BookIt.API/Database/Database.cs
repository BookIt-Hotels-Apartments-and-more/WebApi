using Microsoft.EntityFrameworkCore;

namespace BookIt.Database
{
    using Microsoft.EntityFrameworkCore;
    using BookIt.Entities;

    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}