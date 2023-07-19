using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SamuraiApp.Domain;

namespace SamuraiApp.Data
{
    // DBContext provides logic for EF Core to interact with your database
    public class SamuraiContext : DbContext
    {
        public SamuraiContext()
        {  
        }

        // Context flexible enough to let me determine which provider to use
        // When I'm instantiating the constructor
        public SamuraiContext(DbContextOptions opt)
            : base(opt)
        {
        }

        public DbSet<Samurai> Samurais { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Battle> Battles { get; set; }
        //There is no DBSet for BattleSamurai table
        public DbSet<SamuraiBattleStat> SamuraiBattleStats { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer("Data Source= (localdb)\\MSSQLLocalDB; Initial Catalog=SamuraiAppData",
            //    options=>options.MaxBatchSize(100))
            //    .LogTo(Console.WriteLine, new[] {DbLoggerCategory.Database.Command.Name},
            //    LogLevel.Information)
            //    .EnableSensitiveDataLogging();

            //Logger configuration to output more details, including transaction information
            //optionsBuilder.UseSqlServer("Data Source= (localdb)\\MSSQLLocalDB; Initial Catalog=SamuraiAppData")
            //    .LogTo(Console.WriteLine, new[] { DbLoggerCategory.Database.Command.Name,
            //                                      DbLoggerCategory.Database.Transaction.Name },
            //    LogLevel.Debug)
            //    .EnableSensitiveDataLogging();

            // Database connection changed for Testing module
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source= (localdb)\\MSSQLLocalDB; Initial Catalog=SamuraiTestData");
            }    
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Adding a many to many payload to existing join table and data
            // Uncomment mapping, EF knows to map that class to the join table (playload)

            //Configure many to many relationship with intermediary table
            modelBuilder.Entity<Samurai>()
                .HasMany(s => s.Battles)
                .WithMany(b => b.Samurais)
                .UsingEntity<BattleSamurai>
                (bs => bs.HasOne<Battle>().WithMany(),
                 bs => bs.HasOne<Samurai>().WithMany())
                .Property(bs => bs.DateJoined)
                .HasDefaultValueSql("getDate()");

            //Example if we change the name of the model
            //modelBuilder.Entity<XYZBattleSamurai>().ToTable("BattleSamurai");

            //Change the table name
            modelBuilder.Entity<Horse>().ToTable("Horses");
            //Configure Entity witout key and set ToView to asign this entity to an existing View in the DataBase
            modelBuilder.Entity<SamuraiBattleStat>().HasNoKey().ToView("SamuraiBattleStats");
        }
    }
}
