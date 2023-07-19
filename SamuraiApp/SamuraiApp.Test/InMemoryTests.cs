using Microsoft.EntityFrameworkCore;
using SamuraiApp.Data;
using SamuraiApp.Domain;

namespace SamuraiApp.Test
{
    [TestClass]
    public class InMemoryTests
    {
        [TestMethod]
        public void CanInsertSamuraiIntoDatabase()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseInMemoryDatabase("CanInsertSamurai");

            using (var context = new SamuraiContext(builder.Options))
            {
                var samurai = new Samurai { Name = "Test Samurai" };
                context.Samurais.Add(samurai);
            
                Assert.AreEqual(EntityState.Added, context.Entry(samurai).State);
            }
        }
    }
}
