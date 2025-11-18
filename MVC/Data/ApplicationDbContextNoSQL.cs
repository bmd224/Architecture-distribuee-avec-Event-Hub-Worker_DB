using Microsoft.EntityFrameworkCore;
using MVC.Models;

namespace MVC.Data
{
    //No SQL
    public class ApplicationDbContextNoSQL : DbContext
    {
        public required IConfiguration Configuration { get; set; }

        public ApplicationDbContextNoSQL(DbContextOptions<ApplicationDbContextNoSQL> options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseCosmos(
                    connectionString: Configuration.GetConnectionString("CosmosDB")!,
                    databaseName: "ApplicationDB",
                    cosmosOptionsAction: options =>
                    {
                        options.ConnectionMode(Microsoft.Azure.Cosmos.ConnectionMode.Direct);
                        options.MaxRequestsPerTcpConnection(16);
                        options.MaxTcpConnectionsPerEndpoint(32);
                    });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ajustement de la capacité de la BD
            // modelBuilder.HasAutoscaleThroughput(1000);

            //Création de la hiérarchie des tables

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToContainer("Posts")                   // Specify the container name
                      .HasNoDiscriminator()                   // No discriminator needed
                      .HasPartitionKey(x => x.Id)             // Set the partition key
                      .HasKey(x => x.Id);                     // Set the primary key

                entity.Property(i => i.Id)                    // Configure the ID property
                      .ValueGeneratedOnAdd();                 // Enable auto-increment
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToContainer("Comments")
                        .HasNoDiscriminator()
                        .HasPartitionKey(x => x.PostId)
                        .HasKey(x => x.Id);

                entity.Property(i => i.Id)
                      .ValueGeneratedOnAdd();
            });
        }

        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
    }
}
