using System;
using System.Collections.Generic;

namespace RWCExchange
{
    internal class Bid : IComparable<Bid>
    {
        public DateTime TimeStamp { get; set; }
        public string User { get; set; }
        public double Price { get; set; }

        public int CompareTo(Bid other)
        {
            if (Price < other.Price) return -1;
            if (Price > other.Price) return 1;
            if (TimeStamp < other.TimeStamp) return -1;
            return TimeStamp > other.TimeStamp ? 1 : 0;
        }
    }

    internal class Ask : IComparable<Ask>
    {
        public DateTime TimeStamp { get; set; }
        public string User { get; set; }
        public double Price { get; set; }

        public int CompareTo(Ask other)
        {
            if (Price > other.Price) return -1;
            if (Price < other.Price) return 1;
            if (TimeStamp < other.TimeStamp) return -1;
            return TimeStamp > other.TimeStamp ? 1 : 0;
        }
    }

    public sealed class Exchange
    {
        private static readonly Lazy<Exchange> _instance = new Lazy<Exchange>(()=>new Exchange());

        private Dictionary<string, List<Bid>> _bids;
        private Dictionary<string, List<Ask>> _asks;

        private Dictionary<string, string> _owners; 

        private readonly List<string> _countries; 

        public static Exchange Instance
        {
            get { return _instance.Value; }
        }

        private Exchange()
        {
            _countries = new List<string> { "ARG", "AUS", "CAN", "ENG", "FJI", "FRA", "GEO", "IRE", "ITA", "JPN", "NAM", "NZL", "ROM", "SAM", "SCO", "RSA", "TGA", "URU", "USA", "WAL" };
            _bids = new Dictionary<string, List<Bid>>();
            _asks = new Dictionary<string, List<Ask>>();
            _owners = new Dictionary<string, string>();
            foreach (var c in _countries)
            {
                _bids.Add(c,new List<Bid>());
                _asks.Add(c,new List<Ask>());
                _owners.Add(c,string.Empty);
            }
        }

        public bool IsValidCountry(string country)
        {
            return _countries.Contains(country);
        }

        public bool IsCurrentOwner(string country, string user)
        {
            return _owners[country] == user;
        }

        public bool ChangeOwner(string owner, string country)
        {
            _owners[country] = owner;
            return true;
        }



    }
}