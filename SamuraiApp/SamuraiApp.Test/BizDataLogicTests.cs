using Microsoft.EntityFrameworkCore;
using SamuraiApp.Data;
using SamuraiApp.Domain;

namespace SamuraiApp.Test
{
    [TestClass]
    public class BizDataLogicTests
    {
        [TestMethod]
        public void CanAddSamuraisByName()
        {
            var buider = new DbContextOptionsBuilder();
            buider.UseInMemoryDatabase("CanAddSamuraisByName");

            using var context = new SamuraiContext(buider.Options);
            var bizLogic = new BusinessDataLogic(context);

            var nameList = new string[] { "Kikuhiyo", "Kyuzo", "Rikchi" };

            var result = bizLogic.AddSamuraisByName(nameList);

            Assert.AreEqual(nameList.Length, result);
        }

        [TestMethod]
        public void CanInsertSingleSamurai()
        {
            var buider = new DbContextOptionsBuilder();
            buider.UseInMemoryDatabase("CanAddSamuraisByName");

            using (var context = new SamuraiContext(buider.Options))
            {
                var bizLogic = new BusinessDataLogic(context);
                bizLogic.InsertNewSamurai(new Samurai { Name = "Test Samurai" });
            }

            // Get a fresh and empty context that isn't tracking anything yet
            // But it will connect to the same in-memory database
            using (var context2 = new SamuraiContext(buider.Options))
            {
                Assert.AreEqual(1, context2.Samurais.Count());
            }
        }
    }
}
