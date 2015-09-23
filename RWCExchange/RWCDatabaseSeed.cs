using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using RWCExchange.Models;

namespace RWCExchange
{
    public class RWCDatabaseSeed : DropCreateDatabaseIfModelChanges<RWCDatabaseContext>
    {
        protected override void Seed(RWCDatabaseContext context)
        {
            var countryCodes =  new List<string> { "ARG", "AUS", "CAN", "ENG", "FJI", "FRA", "GEO", "IRE", "ITA", "JPN", "NAM", "NZL", "ROM", "SAM", "SCO", "RSA", "TGA", "URU", "USA", "WAL" };
            context.Countries.AddRange(countryCodes.Select(i => new Country { Code = i }));
            context.SaveChanges();
            var users = new Dictionary<string,ICollection<Country>>
                        {
                            { "jon",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="JPN")}},
                            {"tbone",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="ROM")}},
                            { "samlloyd",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="AUS")}},
                            { "pedro",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="FRA")}},
                            {"james",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="NAM")}},
                            {"darryl",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="URU")}},
                            {"tommy",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="NZL")}},
                            {"siobhan",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="CAN")}},
                            {"brentkelly",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="TGA")}},
                            {"jonny",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="SAM")}},
                            {"joooe",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="GEO"),context.Countries.FirstOrDefault(i=>i.Code=="WAL")}},
                            {"joshuabalfe",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="SCO")}},
                            {"damtur",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="ARG")}},
                            {"stu",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="RSA")}},
                            {"chrislewis",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="ENG")}},
                            {"johnobrien",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="FJI")}},
                            {"vasman",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="IRE")}},
                            {"johnmc",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="ITA")}},
                            {"jjramos",new Collection<Country> {context.Countries.FirstOrDefault(i=>i.Code=="USA")}},
                            {"house",new Collection<Country>()}
                        };
            context.Users.AddRange(users.Select(i => new User{UserName = i.Key, Countries = i.Value}));
            context.SaveChanges();
            base.Seed(context);
        }
    }
}