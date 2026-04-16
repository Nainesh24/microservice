using Microsoft.EntityFrameworkCore;
using UserService.Model;

namespace UserService.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) 
        {
        }
        public DbSet<User> Users { get; set; }
    }
}
