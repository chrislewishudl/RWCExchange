using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using RWCExchange.Models;

namespace RWCExchange
{
    public class RWCDatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Ask> Asks { get; set; }
        public DbSet<Trade> Trades { get; set; }

        public RWCDatabaseContext() : base("RWCDatabase")
        {
            Database.SetInitializer(new RWCDatabaseSeed());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}