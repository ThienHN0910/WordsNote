using Domain.Entities.AS;
using Domain.Entities.DMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocVersion> Versions { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Annotation> Annotations { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        //    var config = new ConfigurationBuilder()
        //        .AddJsonFile($"appsettings.{env}.json", optional: false, reloadOnChange: true)
        //        .Build();

        //    var provider = config["DatabaseProvider"];
        //    var connectionString = config.GetConnectionString("DefaultConnection");

        //    if (!optionsBuilder.IsConfigured)
        //    {
        //        if (provider == "SqlServer")
        //            optionsBuilder.UseSqlServer(connectionString);
        //        else if (provider == "PostgreSQL")
        //            optionsBuilder.UseNpgsql(connectionString);
        //    }
        //}


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Document>()
              .HasMany(p => p.Tags)
              .WithMany(t => t.Documents)
              .UsingEntity(j => j.ToTable("DocTags"));
        }
    }
}
