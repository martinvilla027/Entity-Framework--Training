using Microsoft.EntityFrameworkCore;
using SamuraiApp.Domain;

namespace SamuraiApp.Data
{
    // DBContext provides logic for EF Core to interact with your database
    public class SamuraiContext : DbContext
    {
        // Constructor created to end the configuration in API-Program.cs configuration
        public SamuraiContext(DbContextOptions<SamuraiContext> options)
            :base(options)
        {
            
        }
        public DbSet<Samurai> Samurais { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Battle> Battles { get; set; }
        //There is no DBSet for BattleSamurai table
        public DbSet<SamuraiBattleStat> SamuraiBattleStats { get; set; }

        // I removed the entire overide OnConfiguring method, because that configuration
        // is moved to Program class, incluiding Logging
      
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
