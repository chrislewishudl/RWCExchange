using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using RWCExchange.Models;

namespace RWCExchange
{
    public sealed class Exchange
    {
        private static readonly Lazy<Exchange> _instance = new Lazy<Exchange>(()=>new Exchange());

        public static Exchange Instance => _instance.Value;

        private Exchange()
        {
        }

        public bool IsValidCountry(string country)
        {
            using (var database = new RWCDatabaseContext())
            {
                var countryFound = database.Countries.FirstOrDefault(i => i.Code == country);
                return countryFound != null;
            }
        }

        public bool CountryDropped(string country)
        {
            using (var database = new RWCDatabaseContext())
            {
                var countryFound = database.Countries.FirstOrDefault(i => i.Code == country);
                return countryFound==null || countryFound.IsDropped;
            }
        }

        public bool IsCurrentOwner(string country, int userId)
        {
            using (var database = new RWCDatabaseContext())
            {
                return database.Countries.Include(i=>i.User).FirstOrDefault(i => i.Code == country)
                    ?.User?.UserID == userId;
            }
        }

        public List<Bid> GetBids(string country)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries
                                .Include("Bids.User")
                                .FirstOrDefault(i => i.Code == country);
                return dbCountry?.Bids.OrderByDescending(i=>i.Price).ThenBy(i=>i.TimeStamp).ToList() ?? new List<Bid>();
            }
        }

        public Ask GetAsk(string country)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries
                                .Include("Ask.User")
                                .FirstOrDefault(i => i.Code == country);
                return dbCountry?.Ask;
            }
        }

        public List<KeyValuePair<string, string>> GetOwners()
        {
            using (var database = new RWCDatabaseContext())
            {
                var owners = (from country in database.Countries
                              select new { country.Code, country.User.UserName})
                             .ToList();

                return new List<KeyValuePair<string, string>>(owners.Select(i=>new KeyValuePair<string, string>(i.Code,i.UserName)));
            }
        } 

        public Trade AddBid(string country,Bid bid, out bool updated)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries
                    .Include(i=>i.Bids)
                    .Include(i=>i.Ask)
                    .Include("User.Countries")
                    .First(i => i.Code == country);
                var dbBids = dbCountry.Bids.ToList();
                var dbAsk = dbCountry.Ask;
                var existingForUser = dbBids.FirstOrDefault(i => i.User.UserID == bid.UserID);
                updated = false;
                bid.CountryID = dbCountry.CountryID;
                if (existingForUser != null)
                {
                    existingForUser.TimeStamp = bid.TimeStamp;
                    existingForUser.Price = bid.Price;
                    updated = true;
                    database.SaveChanges();
                    if (dbAsk == null || !dbBids.OrderByDescending(i => i.Price)
                                            .ThenBy(i => i.TimeStamp)
                                            .First()
                                            .Equals(bid))
                    {
                        return null;
                    }
                }
                if (dbAsk == null)
                {
                    dbCountry.Bids.Add(bid);
                    database.SaveChanges();
                    return null;
                }
                if (bid.Price >= dbAsk.Price)
                {
                    var currentSeller = dbCountry.User.UserID;
                    dbCountry.Ask = null;
                    dbCountry.User.Countries.Remove(dbCountry);
                    dbCountry.UserID = bid.UserID;
                    if (updated)
                    {
                        database.Bids.Remove(existingForUser);
                    }
                    var trade = new Trade
                    {
                        BuyerID = bid.UserID,
                        CountryID = dbCountry.CountryID,
                        Price = bid.Price,
                        SellerID =currentSeller,
                        TimeStamp = DateTime.UtcNow
                    };
                    database.Trades.Add(trade);
                    database.SaveChanges();
                    return trade;
                }
                if (updated) return null;
                dbCountry.Bids.Add(bid);
                database.SaveChanges();
                return null;
            }
        }

        public Trade AddAsk(string country, Ask ask, out bool updated)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries
                    .Include(i => i.Bids)
                    .Include(i => i.Ask)
                    .Include("User.Countries")
                    .First(i => i.Code == country);
                var dbBids = dbCountry.Bids.ToList();
                var dbAsk = dbCountry.Ask;
                updated = false;
                if (dbAsk != null && dbCountry.User.UserID==ask.UserID)
                {
                    dbAsk.TimeStamp = ask.TimeStamp;
                    dbAsk.Price = ask.Price;
                    updated = true;
                    database.SaveChanges();
                    if (!dbBids.Any())
                    {
                        return null;
                    }

                }
                ask.CountryID = dbCountry.CountryID;
                if (!dbBids.Any() && dbAsk==null)
                {
                    dbCountry.Ask = ask;
                    database.SaveChanges();
                    return null;
                }
                var highestBid = dbBids.OrderByDescending(i => i.Price)
                                       .ThenBy(i => i.TimeStamp)
                                       .First();
                if (highestBid.Price >= ask.Price)
                {
                    database.Bids.Remove(highestBid);
                    dbCountry.User.Countries.Remove(dbCountry);
                    dbCountry.User = highestBid.User;
                    dbCountry.Ask = null;
                    var trade = new Trade()
                    {
                        BuyerID = highestBid.UserID,
                        SellerID = ask.UserID,
                        CountryID = dbCountry.CountryID,
                        Price = ask.Price,
                        TimeStamp = DateTime.UtcNow
                    };
                    database.Trades.Add(trade);
                    database.SaveChanges();
                    return trade;
                }
                if (updated)return null;
                dbCountry.Ask = ask;
                database.SaveChanges();
                return null;
            }
           
        }

        public Bid GetBestBid(string country)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries.Include("Bids.User").FirstOrDefault(i => i.Code == country);
                return dbCountry==null||!dbCountry.Bids.Any()?null: dbCountry.Bids.OrderByDescending(i=>i.Price).ThenBy(i=>i.TimeStamp).First();
            }
            
        }

        public Ask GetBestAsk(string country)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries.Include("Ask.User").FirstOrDefault(i => i.Code == country);
                return dbCountry?.Ask;
            }
        }

        public string DropTeam(string country)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries
                    .Include(i=>i.Bids)
                    .FirstOrDefault(i => i.Code == country);
                if (dbCountry == null || dbCountry.IsDropped) return null;
                dbCountry.Ask = null;
                database.Bids.RemoveRange(dbCountry.Bids);
                var currentOwner = dbCountry.User?.UserName;
                dbCountry.User = null;
                dbCountry.IsDropped = true;
                database.SaveChanges();
                return currentOwner;
            }
        }

        public bool CurrentlyOwned(string country)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries.FirstOrDefault(i => i.Code == country);
                return dbCountry != null && !dbCountry.IsDropped && dbCountry.User != null;
            }
        }

        public bool PullBid(string country, int userId)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries
                    .Include(i=>i.Bids)
                    .FirstOrDefault(i => i.Code == country);
                if (dbCountry == null || dbCountry.IsDropped) return false;
                var bid = dbCountry.Bids.FirstOrDefault(i => i.UserID == userId);
                if (bid == null) return false;
                database.Bids.Remove(bid);
                database.SaveChanges();
                return true;
            }
        }

        public bool PullAsk(string country, int userId)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries
                    .FirstOrDefault(i => i.Code == country);
                if (dbCountry == null || dbCountry.IsDropped) return false;
                if(dbCountry.Ask==null || dbCountry.User.UserID!=userId) return false;
                dbCountry.Ask = null;
                database.SaveChanges();
                return true;
            }
        }

        public bool SetOwner(string country, int userId)
        {
            using (var database = new RWCDatabaseContext())
            {
                var user = database.Users.Find(userId);
                var dbCountry = database.Countries.FirstOrDefault(i => i.Code == country);
                if (dbCountry == null || dbCountry.IsDropped) return false;
                if (user.UserName == "house")
                {
                    dbCountry.Bids.Clear();
                    dbCountry.Ask = new Ask { Price = 0.01, TimeStamp = DateTime.Now, UserID = user.UserID,CountryID = dbCountry.CountryID};
                }
                user.Countries.Add(dbCountry);
                dbCountry.User = user;
                dbCountry.Ask = null;
                database.SaveChanges();
                return true;
            }

            
        }



    }
}