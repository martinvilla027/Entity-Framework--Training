namespace SamuraiApp.Domain
{
    public class Samurai
    {
        public int Id { get; set; }
        public string Name { get; set; }
        //One to many relationship
        public List<Quote> Quotes { get; set; } = new List<Quote>();
        //Many to many relationship
        public List<Battle> Battles { get; set; } = new List<Battle>();
        //One to one relationship
        public Horse Horse { get; set; }
    }
}
