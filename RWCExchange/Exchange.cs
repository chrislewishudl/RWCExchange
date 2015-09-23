using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Antlr.Runtime.Misc;
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
                return database.Countries.FirstOrDefault(i => i.Code == country)
                    ?.User?.UserID == userId;
            }
        }

        public List<Bid> GetBids(string country)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries.FirstOrDefault(i => i.Code == country);
                return dbCountry?.Bids.ToList() ?? new List<Bid>();
            }
        }

        public Ask GetAsk(string country)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries.FirstOrDefault(i => i.Code == country);
                return dbCountry?.Ask;
            }
        }

        public List<KeyValuePair<string, string>> GetOwners()
        {
            using (var database = new RWCDatabaseContext())
            {
                return database.Countries.Select(i => new KeyValuePair<string, string>(i.Code, i.User.UserName))
                                .ToList();
            }
        } 

        public Trade AddBid(string country,Bid bid, out bool updated)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries.First(i => i.Code == country);
                var dbBids = dbCountry.Bids.ToList();
                var dbAsk = dbCountry.Ask;
                var existingForUser = dbBids.FirstOrDefault(i => i.User.UserID == bid.User.UserID);
                updated = false;
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
                bid.CountryID = dbCountry.CountryID;
                if (dbAsk == null)
                {
                    dbBids.Add(bid);
                    database.SaveChanges();
                    return null;
                }
                if (bid.Price >= dbAsk.Price)
                {
                    var currentSeller = dbCountry.User.UserID;
                    dbCountry.Ask = null;
                    dbCountry.User.Country = null;
                    dbCountry.User = bid.User;
                    if (updated)
                    {
                        dbCountry.Bids.Remove(existingForUser);
                    }
                    var trade = new Trade
                    {
                        BuyerID = bid.UserID,
                        Buyer = bid.User,
                        CountryID = dbCountry.CountryID,
                        Price = bid.Price,
                        SellerID =currentSeller,
                        Seller = dbCountry.User
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
                var dbCountry = database.Countries.First(i => i.Code == country);
                var dbBids = dbCountry.Bids.ToList();
                var dbAsk = dbCountry.Ask;
                updated = false;
                if (dbAsk != null && dbCountry.User.UserID==ask.User.UserID)
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
                    dbBids.Remove(highestBid);
                    dbCountry.User.Country = null;
                    dbCountry.User = highestBid.User;
                    dbCountry.Ask = null;
                    var trade = new Trade()
                    {
                        BuyerID = highestBid.UserID,
                        Buyer = highestBid.User,
                        SellerID = ask.UserID,
                        Seller = ask.User,
                        CountryID = dbCountry.CountryID,
                        Price = ask.Price
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
                var dbCountry = database.Countries.FirstOrDefault(i => i.Code == country);
                return dbCountry==null||!dbCountry.Bids.Any()?null: dbCountry.Bids.First();
            }
            
        }

        public Ask GetBestAsk(string country)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries.FirstOrDefault(i => i.Code == country);
                return dbCountry?.Ask;
            }
        }

        public string DropTeam(string country)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries.FirstOrDefault(i => i.Code == country);
                if (dbCountry == null || dbCountry.IsDropped) return null;
                dbCountry.Ask = null;
                dbCountry.Bids.Clear();
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
                var dbCountry = database.Countries.FirstOrDefault(i => i.Code == country);
                if (dbCountry == null || dbCountry.IsDropped) return false;
                var bid = dbCountry.Bids.FirstOrDefault(i => i.UserID == userId);
                if (bid == null) return false;
                dbCountry.Bids.Remove(bid);
                database.SaveChanges();
                return true;
            }
        }

        public bool PullAsk(string country, string user)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries.FirstOrDefault(i => i.Code == country);
                if (dbCountry == null || dbCountry.IsDropped) return false;
                if(dbCountry.Ask==null || dbCountry.User.UserName!=user) return false;
                dbCountry.Ask = null;
                database.SaveChanges();
                return true;
            }

        }

        public bool SetOwner(string country, User user)
        {
            using (var database = new RWCDatabaseContext())
            {
                var dbCountry = database.Countries.FirstOrDefault(i => i.Code == country);
                if (dbCountry == null || dbCountry.IsDropped) return false;
                if (user.UserName == "house")
                {
                    dbCountry.Bids.Clear();
                    dbCountry.Ask = new Ask { Price = 0.01, TimeStamp = DateTime.Now, UserID = user.UserID,CountryID = dbCountry.CountryID};
                }
                dbCountry.User = user;
                database.SaveChanges();
                return true;
            }

            
        }



    }
}