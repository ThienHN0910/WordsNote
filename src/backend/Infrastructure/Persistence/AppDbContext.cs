using Domain.Entities.AS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
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
        }
    }
}
