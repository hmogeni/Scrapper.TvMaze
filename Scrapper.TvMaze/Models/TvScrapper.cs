using System.Collections.Generic;

namespace Scrapper.TvMaze.Models
{
    public class Episode
    {
        public long id { get; set; }

        public Embed _embedded { get; set; }
    }

    public class Embed
    {
        public Show show { get; set; }
    }

    public class Person
    {
        public Cast person { get; set; }
    }

    public class TvShow
    {
        public long id { get; set; }

        public string name { get; set; }

        public List<TvCast> cast { get; set; }
    }

    public class TvCast
    {
        public long id { get; set; }

        public string name { get; set; }

        public string birthday { get; set; }
    }
}
