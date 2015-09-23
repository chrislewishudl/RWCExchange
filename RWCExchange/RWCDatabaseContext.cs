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
            modelBuilder.Entity<Country>()
                        .HasOptional(i => i.Ask)
                        .WithRequired(i => i.Country);
            modelBuilder.Entity<Country>()
                        .HasOptional(i => i.User)
                        .WithMany(i => i.Countries)
                        .HasForeignKey(i=>i.UserID)
                        .WillCascadeOnDelete(false);
            modelBuilder.Entity<Bid>()
                        .HasRequired(i => i.Country)
                        .WithMany(i => i.Bids)
                        .HasForeignKey(i => i.CountryID)
                        .WillCascadeOnDelete(true);
            modelBuilder.Entity<Bid>()
                        .HasRequired(i => i.User);
            modelBuilder.Entity<Ask>()
                        .HasRequired(i => i.User);
            modelBuilder.Entity<Bid>()
                        .HasRequired(i => i.Country);
            modelBuilder.Entity<Ask>()
                        .HasRequired(i => i.Country);
            modelBuilder.Entity<Trade>()
                        .HasRequired(i => i.Country);
            modelBuilder.Entity<Trade>()
                        .HasRequired(i => i.Buyer)
                        .WithMany(i=>i.BuyTrades)
                        .HasForeignKey(i=>i.BuyerID)
                        .WillCascadeOnDelete(false);
            modelBuilder.Entity<Trade>()
                        .HasRequired(i => i.Seller)
                        .WithMany(i=>i.SellTrades)
                        .HasForeignKey(i=>i.SellerID)
                        .WillCascadeOnDelete(false);
        }
    }
}