using SamuraiApp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamuraiApp.Data
{
    // Class created in Testing module
    public class BusinessDataLogic
    {
        private SamuraiContext _context;

        public BusinessDataLogic(SamuraiContext context)
        {
            _context = context;
        }

        public BusinessDataLogic()
        {
            _context = new SamuraiContext();
        }

        public int AddSamuraisByName(params string[] names)
        {
            foreach (var name in names)
            {
                _context.Samurais.Add(new Samurai { Name = name });
            }

            var dbResult = _context.SaveChanges();
            return dbResult;
        }

        public int InsertNewSamurai(Samurai samurai)
        {
            _context.Samurais.Add(samurai);
            var dbResult = _context.SaveChanges();
            return dbResult;
        }
    }
}
