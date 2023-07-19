using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SamuraiApp.Data;
using SamuraiApp.Domain;
using System.Net.Mail;

namespace SamuraiApp.UI
{
    internal class Program
    {
        private static SamuraiContext _context = new SamuraiContext();
        private static SamuraiContext _contectNT = new SamuraiContextNoTracking();

        static void Main(string[] args)
        {
            //_context.Database.EnsureCreated();

            //AddSamuraisByName("Shimada", "Okamoto", "Kikuchio", "Hayashida");
            //GetSamurais();
            //GetSamurais("After Add:");
            //QueryAndUpdateBattles_Disconnected();
            //Console.Write("Press any key...");
            //Console.ReadKey();

            QueryUsingFromSqlRawStoredProc();
        }

        #region ADD

        private static void AddVariousTypes()
        {
            _context.AddRange(new Samurai { Name = "Shimada" },
                            new Samurai { Name = "Okamoto" },
                            new Battle { Name = "Battle of Anegawa" },
                            new Battle { Name = "Battle of Nagashiro" });

            //_context.Samurais.AddRange(
            //    new Samurai { Name = "Shimada" },
            //    new Samurai { Name = "Okamoto" });

            //_context.Battles.AddRange(
            //    new Battle { Name = "Battle of Anegawa" },
            //    new Battle { Name = "Battle of Nagashiro" });

            _context.SaveChanges();
        }

        private static void AddSamuraisByName(params string[] names)
        {
            foreach (var name in names)
            {
                _context.Samurais.Add(new Samurai { Name = name });
            }
            _context.SaveChanges();
        }

        #endregion

        #region GET

        private static void GetSamurais()
        {
            var samurais = _context.Samurais
            .TagWith("ConsoleApp.Program.GetSamurais method")
            .ToList();
            Console.WriteLine($"Samurai count is {samurais.Count}");
            foreach (var samurai in samurais)
            {
                Console.WriteLine(samurai.Name);
            }
        }

        #endregion

        #region GET-Filters

        private static void QueryFilters()
        {
            //var name = "Sampson";
            // Logging doesn't show name value by EnableSensitiveDataLogging() configuration in the db context
            //var filteredSamurais = _context.Samurais.Where(s => s.Name == name).ToList();

            var filter = "J%";
            // Add no tracking context to check output logs
            var likeSamurais = _contectNT.Samurais
                                .Where(s => EF.Functions.Like(s.Name, filter)).ToList();
        }

        private static void QueryAggregates()
        {
            //var name = "Sampson";
            //var filteredSamurai = _context.Samurais.FirstOrDefault(s => s.Name == name);

            // Not a LINQ method, executes immediately (DbSet.Find(key))
            // Add no tracking context to check output logs
            var filteredSamurai2 = _contectNT.Samurais.Find(2);
        }

        #endregion

        #region UPDATE

        private static void RetrieveAndUpdateSamurai()
        {
            var samurai = _context.Samurais.FirstOrDefault();
            samurai.Name += "San";
            _context.SaveChanges();
        }

        private static void RetrieveAndUpdateMultipleSamurais()
        {
            // Skip and Take combo are great for paging data
            // I skipped the first one, I already modified it
            var samurais = _context.Samurais.Skip(1).Take(4).ToList();
            samurais.ForEach(s => s.Name += "San");
            _context.SaveChanges();
        }

        private static void MultipleDatabaseOperations()
        {
            var samurai = _context.Samurais.FirstOrDefault();
            samurai.Name += "San";
            _context.Samurais.Add(new Samurai { Name = "Shino" });
            _context.SaveChanges();
        }

        #endregion

        #region DELETE

        private static void RetrieveAndDeleteASamurai()
        {
            var samurai = _context.Samurais.Find(18);
            _context.Remove(samurai);
            _context.SaveChanges();
        }

        #endregion

        #region DISCONNECTED-CONTEXT

        private static void QueryAndUpdateBattles_Disconnected()
        {
            List<Battle> disconnectedBattles;
            using (var context1 = new SamuraiContext())
            {
                disconnectedBattles = _context.Battles.ToList();
            } // context1 is disposed

            disconnectedBattles.ForEach(b =>
            {
                b.StartDate = new DateTime(1570, 01, 01);
                b.EndDate = new DateTime(1570, 12, 1);
            });

            using (var context2 = new SamuraiContext())
            {
                context2.UpdateRange(disconnectedBattles);
                context2.SaveChanges();
            }
        }

        #endregion

        #region ADD-RELATED-DATA

        private static void InsertNewSamuraiWithAQuote()
        {
            var samurai = new Samurai
            {
                Name = "Kambei Shimada",
                Quotes = new List<Quote>
                {
                    new Quote {Text = "I've come to save you"}
                }
            };

            _context.Samurais.Add(samurai);
            _context.SaveChanges();
        }

        private static void InsertNewSamuraiWithManyQuotes()
        {
            var samurai = new Samurai
            {
                Name = "Kyuzo",
                Quotes = new List<Quote>
                {
                    new Quote {Text = "Watch out for my sharp sword!"},
                    new Quote {Text = "I told you to watch out for the sharp sword! Oh well!"}
                }
            };

            _context.Samurais.Add(samurai);
            _context.SaveChanges();
        }

        private static void AddQuoteToExistingSamuraiWhileTracked()
        {
            var samurai = _context.Samurais.FirstOrDefault();
            samurai.Quotes.Add(new Quote
            {
                Text = "I bet you're happy that I've saved you"
            });
            _context.SaveChanges();
        }

        private static void AddQuoteToExistingSamuraiNotTracked(int samuraiId)
        {
            var samurai = _context.Samurais.Find(samuraiId);
            samurai.Quotes.Add(new Quote
            {
                Text = "Now that I saved you, will you feed me dinner?"
            });

            //New DbContext in disconnected scenario
            using (var newContext = new SamuraiContext())
            {
                //Use Attach instead of Update
                //Attach just connects the object and sets it's state to unmodified
                //Keep in mind in disconnected scenarios
                newContext.Samurais.Attach(samurai);
                newContext.SaveChanges();
            }
        }

        private static void Simpler_AddQuoteToExistingSamuraiNotTracked(int samuraiId)
        {
            var quote = new Quote { Text = "Thanks for dinner!", SamuraiId = samuraiId };
            //using declaration gets disposed when variable goes out of scope
            using var newContext = new SamuraiContext();
            newContext.Quotes.Add(quote);
            newContext.SaveChanges();
        }

        #endregion

        #region LOAD-RELATED-DATA

        //Include related objects in query - internally SQL do a LEFT JOIN 
        private static void EagerLoadSamuraiWithQuotes()
        {
            //var samuraiWithQuotes = _context.Samurais.Include(s => s.Quotes).ToList();

            // SplitQuery => query is broken up into multiple queries sent in a single command
            //var splitQuery = _context.Samurais.AsSplitQuery().Include(s => s.Quotes).ToList();

            //var filteredInclude = _context.Samurais
            //    .Include(s => s.Quotes.Where(q => q.Text.Contains("Thanks"))).ToList();

            var filterPrimaryEntityWithInclude =
                _context.Samurais.Where(s => s.Name.Contains("Sampson"))
                .Include(s => s.Quotes).FirstOrDefault();
        }

        //Query Projections - Define the shape of query results
        private static void ProjectSomeProperties()
        {
            // This will return what's known in .NET as an anonymous type
            var someProperties = _context.Samurais.Select(s => new { s.Id, s.Name }).ToList();
            // Casting to a list of defined types
            var idAndNames = _context.Samurais.Select(s => new IdAndName(s.Id, s.Name)).ToList();
        }

        public struct IdAndName
        {
            public IdAndName(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public int Id;
            public string Name;
        }

        private static void ProjectSamuraisWithQuotes()
        {
            //var somePropsWithQuotes = _context.Samurais
            //    .Select(s => new { s.Id, s.Name, NumberOfQueotes = s.Quotes.Count })
            //    .ToList();

            var somePropsWithQuotes = _context.Samurais
                .Select(s => new { s.Id, s.Name, HappyQuotes = s.Quotes.Where(q => q.Text.Contains("happy")) })
                .ToList();

            // It shows a circular reference between related objects
            var samuraisAndQuotes = _context.Samurais
                .Select(s => new
                {
                    Samurai = s,
                    HappyQuotes = s.Quotes.Where(q => q.Text.Contains("happy"))
                })
                .ToList();

            var firstSamurai = samuraisAndQuotes[0].Samurai.Name += " The happiest";
        }

        // Explicit Loading - Request related data of objects in memory
        private static void ExplicitLoadQuotes()
        {
            //make sure there's a horse in the DB, then clear the context's change tracker
            _context.Set<Horse>().Add(new Horse { SamuraiId = 1, Name = "Mr. Ed" });
            _context.SaveChanges();
            _context.ChangeTracker.Clear();
            //----------------------------------
            // You can only load from a single object
            var samurai = _context.Samurais.Find(1);
            _context.Entry(samurai).Collection(s => s.Quotes).Load();
            _context.Entry(samurai).Reference(s => s.Horse).Load();
        }

        // Lazy Loading - On-the-fly retrieval of related data
        private static void LazyLoadingQuotes()
        {
            // By default is disabled
            // Enable with these requrements:
            // Every navegation property in every entity must be virtual
            // Microsoft.EntityFramework.Proxies package
            // OnConfiguring optionsBuilder.UseLazyLoadingProxies()

            var samurai = _context.Samurais.Find(2);
            var quoteCount = samurai.Quotes.Count(); // won't run without LL setup 
        }

        #endregion

        #region FILTER-RELATED-DATA

        private static void FiteringWithRelatedData()
        {
            var samurais = _context.Samurais
                .Where(s => s.Quotes.Any(q => q.Text.Contains("happy")))
                .ToList();
        }

        #endregion

        #region MODIFY-RELATED-DATA

        private static void ModifyingRelatedDataWhenTracked()
        {
            var samurai = _context.Samurais.Include(s => s.Quotes)
                .FirstOrDefault(s => s.Id == 2);
            samurai.Quotes[0].Text = "Did you hear that?";
            _context.SaveChanges();
        }

        private static void ModifyingRelatedDataWhenNotTracked()
        {
            var samurai = _context.Samurais.Include(s => s.Quotes)
                .FirstOrDefault(s => s.Id == 2);
            var quote = samurai.Quotes[0];
            quote.Text = "Did you hear that again?";

            using var newContext = new SamuraiContext();
            //newContext.Quotes.Update(quote);
            newContext.Entry(quote).State = EntityState.Modified;
            newContext.SaveChanges();
        }

        #endregion

        #region GETTING-AND-ADDING-MANY-TO-MANY-JOINS

        private static void AddingNewSamuraiToAnExistingBattle()
        {
            var battle = _context.Battles.FirstOrDefault();
            battle.Samurais.Add(new Samurai { Name = "Takeda Shingen" });
            _context.SaveChanges();
        }

        private static void ReturnBattleWithSamurais()
        {
            //Use Include to return battle with samurais, I don't have to know abour the join
            //EF internally do joins, check relationships and return the data
            var battle = _context.Battles.Include(b => b.Samurais).FirstOrDefault();
        }

        private static void ReturnAllBattleWithSamurais()
        {
            //Use Include to return battle with samurais, I don't have to know abour the join
            //EF internally do joins, check relationships and return the data
            var battle = _context.Battles.Include(b => b.Samurais).ToList();
        }

        private static void AddAllSamuraisToAllBattles()
        {
            var allBattles = _context.Battles.ToList();
            //I filtered samurais because there is an error joining the two tables
            // I already joined the samurai with Id = 16
            var allSamurais = _context.Samurais.Where(s => s.Id != 16).ToList();

            foreach (var batlle in allBattles)
            {
                batlle.Samurais.AddRange(allSamurais);
            }

            _context.SaveChanges();
        }

        #endregion

        #region ALTERING-OR-REMOVING-MANY-TO-MANY-JOINS

        // Remove the original join
        // Then create the new join
        private static void RemoveSamuraiFromABattle()
        {
            // Retrieve one set of data that represents one samurai in one battle
            // Query the battle and include the samurai whose ID is 16
            var battleWithSamurai = _context.Battles
                .Include(b => b.Samurais.Where(s => s.Id == 16))
                .Single(s => s.BattleId == 1);

            var samurai = battleWithSamurai.Samurais[0];
            // Remove this samurai from the battle's samurais list
            battleWithSamurai.Samurais.Remove(samurai);

            _context.SaveChanges(); // the relationship is not being tracked
        }

        private static void WillNotRemoveSamuraiFromABattle()
        {
            var battle = _context.Battles.Find(1);
            var samurai = _context.Samurais.Find(12);

            battle.Samurais.Remove(samurai);
            _context.SaveChanges(); // the relationship is not being tracked
        }

        #endregion

        #region WORKING-WITH-MANY-TO-MANY-PAYLOAD-DATA

        private static void RemoveSamuraiFromABattleExplicit()
        {
            // Retrieve he joined data with context.Set<BattleSamurai>()
            var b_s = _context.Set<BattleSamurai>()
                .SingleOrDefault(bs => bs.BattleId == 1 && bs.SamuraiId == 14);

            if (b_s != null) 
            {
                _context.Remove(b_s); // _context.Set<BattleSamurai>().Remove works, too
                _context.SaveChanges();
            }
        }

        #endregion

        #region PERSISTING-DATA-IN-ONE-TO-ONE-RELATIONSHIPS

        private static void AddNewSamuraiWithHorse()
        {
            var samurai = new Samurai { Name = "Jina Ujichika" };
            samurai.Horse = new Horse { Name = "Silver" };
            _context.Samurais.Add(samurai);
            _context.SaveChanges();
        }

        private static void AddNewHorseToSamuraiUsingId()
        {
            var horse = new Horse { Name = "Scout", SamuraiId = 2 };
            _context.Add(horse);
            _context.SaveChanges();
        }

        // FALTAN ESTOS DOS MÉTODOS, VALIDAR IDS ANTES DE EJECUTAR
        private static void AddNewHorseToSamuraiObject()
        {
            var samurai = _context.Samurais.Find(16);
            samurai.Horse = new Horse { Name = "Black Beauty" };
            _context.SaveChanges();
        }

        private static void AddNewHorseToDisconnectedSamuraiObject()
        {
            var samurai = _context.Samurais.AsNoTracking().FirstOrDefault(s => s.Id == 7);
            samurai.Horse = new Horse { Name = "Mr. Ed" };

            using var newContext = new SamuraiContext();
            newContext.Samurais.Attach(samurai);
            newContext.SaveChanges();
        }

        private static void ReplaceHorse()
        {
            var samurai = _context.Samurais.Include(s => s.Horse)
                                    .FirstOrDefault(s => s.Id == 7);
            samurai.Horse = new Horse { Name = "Trigger" };
            _context.SaveChanges();
        }

        #endregion

        #region QUERYING-ONE-TO-ONE-RELATIONSHIPS

        private static void GetSamuraiWithHorse()
        {
            var samurais = _context.Samurais.Include(s => s.Horse).ToList();
        }

        private static void GetHorsesWithSamurai()
        {
            // You can create the SbSet for Horses
            var horseOnly = _context.Set<Horse>().Find(3);

            var horseWithSamurai = _context.Samurais.Include(s => s.Horse)
                                            .FirstOrDefault(s => s.Horse.Id == 3);

            var horseSamuraiPairs = _context.Samurais
                    .Where(s => s.Horse != null)
                    .Select(s => new { Horse = s.Horse, Samurai = s })
                    .ToList();
        }

        #endregion

        #region QUERYING-DATABASE-VIEWS

        private static void QuerySamuraiBattleStats()
        {
            // We call by the context the existing view in the database
            var stats = _context.SamuraiBattleStats.ToList();

            var firststat = _context.SamuraiBattleStats.FirstOrDefault();
            var sampsonState = _context.SamuraiBattleStats
                .FirstOrDefault(b => b.Name == "SampsonSan");

            // There is an error because SamuraiBattleStats doesn't have key
            var findone = _context.SamuraiBattleStats.Find(2);
        }

        #endregion

        #region QUERYING-WITH-RAW-SQL

        private static void QueryUsingRawSql()
        {
            // Must return data for all properties of the entity type
            var samurais = _context.Samurais.FromSqlRaw("Select * from samurais").ToList();
        }

        private static void QueryRelatedUsingRawSql()
        {
            var samurais = _context.Samurais.FromSqlRaw(
                "Select Id, Name from Samurais").Include(s => s.Quotes).ToList();
        }

        private static void QueryUsingRawSqlWithInterpolation()
        {
            // DON' DO, Sql Injection attack!
            var name = "kikuchyo";
            var samurais = _context.Samurais
                .FromSqlInterpolated($"Select * from Samurais Where Name = '{name}'")
                .ToList();
        }

        #endregion

        #region RUNNING-STORED-PROCEDURE-QUERIES-WITH-RAW-SQL

        private static void QueryUsingFromSqlRawStoredProc()
        {
            var text = "Happy";
            var samurais = _context.Samurais.FromSqlRaw("EXEC dbo.SamuraisWhoSaidAWord {0}", text).ToList();
        }

        private static void QueryUsingFromSqlIntStoredProc()
        {
            var text = "Happy";
            var samurais = _context.Samurais.FromSqlInterpolated
                ($"EXEC dbo.SamuraisWhoSaidAWord {text}").ToList();
        }

        #endregion

        #region EXECUTING-NON-QUERY-RAW-SQL-COMMANDS

        private static void ExecuteSomeRawSql()
        {
            // Database accesses the db configured for the DbContext instance
            // Execute sql queries but returning only the number of affected rows
            var samuraiId = 2;
            var affectedRows = _context.Database
                .ExecuteSqlRaw("EXEC DeleteQuotesForSamurai {0}", samuraiId);
        }

        #endregion

        #region TESTING

        private static void AddSamuraisByNameTesting(params string[] names)
        {
            var _bizData = new BusinessDataLogic();
            var newSamuraisCreatedCount = _bizData.AddSamuraisByName(names);
        }

        #endregion
    }
}