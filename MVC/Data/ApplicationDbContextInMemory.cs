
using Microsoft.EntityFrameworkCore;
using MVC.Models;


namespace MVC.Data
{
    public class ApplicationDbContextInMemory : DbContext
    {
        public required IConfiguration Configuration { get; set; }
        public ApplicationDbContextInMemory(DbContextOptions<ApplicationDbContextInMemory> options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
    }
}
