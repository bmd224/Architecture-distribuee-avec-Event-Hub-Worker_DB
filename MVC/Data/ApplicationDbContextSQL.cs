
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MVC.Models;



namespace MVC.Data
{
    //dotnet ef migrations add InitialCreate -c ApplicationDbContext
    //dotnet ef migrations add "MigrationName" -c ApplicationDbContext
    //dotnet ef database update -c ContextName

    //dotnet ef database update 0 --context ApplicationDbContext


    //SQL
    public class ApplicationDbContextSQL : DbContext
    {
        public required IConfiguration Configuration { get; set; }

        public ApplicationDbContextSQL(DbContextOptions<ApplicationDbContextSQL> options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                .UseSqlServer(Configuration.GetConnectionString("LocalSQL")!)
                .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Trace)
                .EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //Création de la hiérarchie des tables
            modelBuilder.Entity<Post>()
                .ToTable("Posts")
                .HasMany(m => m.Comments)
                .WithOne(m => m.Post)
                .HasForeignKey(m => m.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .ToTable("Comments");

        }

        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
    }
}
