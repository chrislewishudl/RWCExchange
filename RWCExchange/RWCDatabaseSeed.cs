using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using RWCExchange.Models;

namespace RWCExchange
{
    public class RWCDatabaseSeed : CreateDatabaseIfNotExists<RWCDatabaseContext>
    {
        protected override void Seed(RWCDatabaseContext context)
        {
            var countryCodes =  new List<string> { "ARG", "AUS", "CAN", "ENG", "FJI", "FRA", "GEO", "IRE", "ITA", "JPN", "NAM", "NZL", "ROM", "SAM", "SCO", "RSA", "TGA", "URU", "USA", "WAL" };
            context.Countries.AddRange(countryCodes.Select(i => new Country { Code = i }));
            context.SaveChanges();
            var users = new Dictionary<string,Country>
                        {
                            { "jon",context.Countries.FirstOrDefault(i=>i.Code=="JPN")},
                            {"tbone",context.Countries.FirstOrDefault(i=>i.Code=="ROM")},
                            { "samlloyd",context.Countries.FirstOrDefault(i=>i.Code=="AUS")},
                            { "pedro",context.Countries.FirstOrDefault(i=>i.Code=="FRA")},
                            {"james",context.Countries.FirstOrDefault(i=>i.Code=="NAM")},
                            {"darryl",context.Countries.FirstOrDefault(i=>i.Code=="URU")},
                            {"tommy",context.Countries.FirstOrDefault(i=>i.Code=="NZL")},
                            {"siobhan",context.Countries.FirstOrDefault(i=>i.Code=="CAN")},
                            {"brentkelly",context.Countries.FirstOrDefault(i=>i.Code=="TGA")},
                            {"jonny",context.Countries.FirstOrDefault(i=>i.Code=="SAM")},
                            {"joooe",context.Countries.FirstOrDefault(i=>i.Code=="GEO")},
                            {"joshuabalfe",context.Countries.FirstOrDefault(i=>i.Code=="SCO")},
                            {"damtur",context.Countries.FirstOrDefault(i=>i.Code=="ARG")},
                            {"stu",context.Countries.FirstOrDefault(i=>i.Code=="RSA")},
                            {"chrislewis",context.Countries.FirstOrDefault(i=>i.Code=="ENG")},
                            {"johnobrien",context.Countries.FirstOrDefault(i=>i.Code=="FJI")},
                            {"vasman",context.Countries.FirstOrDefault(i=>i.Code=="IRE")},
                            {"johnmc",context.Countries.FirstOrDefault(i=>i.Code=="ITA")},
                            {"jjramos",context.Countries.FirstOrDefault(i=>i.Code=="USA")},
                            {"joooe",context.Countries.FirstOrDefault(i=>i.Code=="WAL")},
                        };
            context.Users.AddRange(users.Select(i => new User() {UserName = i.Key, CountryID = i.Value.CountryID}));
            context.SaveChanges();
            base.Seed(context);
        }
    }
}