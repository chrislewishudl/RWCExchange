using System;
using System.Collections.Generic;
using System.Linq;
using RWCExchange.Models;

namespace RWCExchange
{
    public sealed class Exchange
    {
        private static readonly Lazy<Exchange> _instance = new Lazy<Exchange>(()=>new Exchange(new RWCDatabaseContext()));

//        private readonly Dictionary<string, List<Bid>> _bids;
//        private readonly Dictionary<string, List<Ask>> _asks;
//
//        private readonly Dictionary<string, string> _owners; 
//
//        private readonly List<string> _countries; 

        public static Exchange Instance => _instance.Value;
        private RWCDatabaseContext _database;

        private Exchange(RWCDatabaseContext context)
        {
            _database = context;
//            _bids = new Dictionary<string, List<Bid>>();
//            _asks = new Dictionary<string, List<Ask>>();
//            _owners = new Dictionary<string, string>();
//            foreach (var c in _countries)
//            {
//                _bids.Add(c,new List<Bid>());
//                _asks.Add(c,new List<Ask>());
//                _owners.Add(c,string.Empty);
//            }
        }

        public bool IsValidCountry(string country)
        {
            var countryFound = _database.Countries.FirstOrDefault(i=>i.Code==country);
            return countryFound!=null;
        }

        public bool CountryDropped(string country)
        {
            var countryFound =_database.Countries.FirstOrDefault(i=>i.Code==country);
            return countryFound?.User == null;
        }

        public bool IsCurrentOwner(string country, string user)
        {
            return _database.Countries.FirstOrDefault(i => i.Code == country)?.User?.UserName == user;
        }

        public List<Bid> GetBids(string country)
        {
            var dbCountry = _database.Countries.FirstOrDefault(i => i.Code == country);
            return dbCountry?.Bids.ToList() ?? new List<Bid>();
        }

        public List<Ask> GetAsks(string country)
        {
            var dbCountry = _database.Countries.FirstOrDefault(i => i.Code == country);
            return dbCountry?.Asks.ToList() ?? new List<Ask>();
        }

        public List<KeyValuePair<string, string>> GetOwners()
        {
            return _database.Countries.Select(i=>new KeyValuePair<string,string>(i.Code,i.User.UserName)).ToList();
        } 

        public Trade AddBid(string country,Bid bid, out bool updated)
        {
            var dbCountry = _database.Countries.First(i => i.Code == country);
            var bids = dbCountry.Bids.ToList();
            var ask = dbCountry.Ask;
            var existingForUser = bids.FirstOrDefault(i=>i.User.UserName==bid.User.UserName);
            updated = false;
            if (existingForUser != null)
            {
                existingForUser.TimeStamp = bid.TimeStamp;
                existingForUser.Price = bid.Price;
                updated = true;
                _database.SaveChanges();
                if (ask==null || !bids.OrderByDescending(i=>i.Price).ThenBy(i=>i.TimeStamp).First().Equals(bid))
                {
                    return null;
                }
            }
            bid.Country = dbCountry;
            if (ask==null && !updated)
            {
                bids.Add(bid);
                return null;
            }
            if (bid.Price >= ask.Price)
            {
                _asks[country].Clear();
                _owners[country] = bid.User;
                trade.Bid = bid;
                trade.Ask = lowestAsk;
                trade.Price = bid.Price;
                trade.Update = false;
                return trade;
            }
            if (!trade.Update)
            {
                _bids[country].Add(bid);
                _bids[country].Sort();
            }
            return trade;
        }

        public Trade AddAsk(string country, Ask ask)
        {
            var existingForUser = _asks[country].FirstOrDefault(i => i.User == ask.User);
            var trade = new Trade();
            if (existingForUser != null)
            {
                existingForUser.TimeStamp = ask.TimeStamp;
                existingForUser.Price = ask.Price;
                trade.Update = true;
                if (!_bids[country].Any())
                {
                    return trade;
                }
            }
            if (!_bids[country].Any() && !trade.Update)
            {
                _asks[country].Add(ask);
                return trade;
            }
            var highestBid = _bids[country][0];
            if (highestBid.Price >= ask.Price)
            {
                _bids[country].Remove(highestBid);
                _owners[country] = highestBid.User;
                trade.Bid = highestBid;
                trade.Ask = ask;
                trade.Price = ask.Price;
                trade.Update = false;
                return trade;
            }
            if (!trade.Update)
            {
                _asks[country].Add(ask);
            }
            return trade;
        }

        public Bid GetBestBid(string country)
        {
            return !_bids[country].Any()?null:_bids[country][0];
        }

        public Ask GetBestAsk(string country)
        {
            return !_asks[country].Any()?null:_asks[country][0];
        }

        public string DropTeam(string country)
        {
            _bids[country].Clear();
            _asks[country].Clear();
            var current = _owners[country];
            _owners.Remove(country);
            return current;
        }

        public bool CurrentlyOwned(string country)
        {
            return _owners[country] != string.Empty;
        }

        public bool PullBid(string country, string user)
        {
            var bid = _bids[country].FirstOrDefault(i => i.User == user);
            if (bid == null) return false;
            _bids[country].Remove(bid);
            _bids[country].Sort();
            return true;
        }

        public bool PullAsk(string country, string user)
        {
            var ask = _asks[country].FirstOrDefault(i => i.User == user);
            if (ask == null) return false;
            _asks[country].Remove(ask);
            return true;
        }

        public bool SetOwner(string country, string user)
        {
            _owners[country] = user;
            if (user == "house")
            {
                _asks[country].Clear();
                _asks[country].Add(new Ask() {Price = 0.01,TimeStamp = DateTime.Now,User = "house"});
            }
            return true;
        }



    }
}