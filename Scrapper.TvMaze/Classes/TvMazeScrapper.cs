using Scrapper.TvMaze.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Scrapper.TvMaze.Classes
{
    public class TvMazeScrapper
    {
        private readonly ScrapperContext _db;

        public TvMazeScrapper()
        {
            string connection = "Data Source=.;Initial Catalog=TvScrapper;Integrated Security=True;";
            _db = new ScrapperContext(connection);
        }

        public async Task<List<Episode>> ShowsAsync(string endPoint)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
            var serializer = new DataContractJsonSerializer(typeof(List<Episode>));
            var streamTask = client.GetStreamAsync(endPoint);
            return serializer.ReadObject(await streamTask) as List<Episode>;
        }
        public async Task<List<Person>> CastAsync(string endPoint)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
            var serializer = new DataContractJsonSerializer(typeof(List<Person>));
            var streamTask = client.GetStreamAsync(endPoint);
            return serializer.ReadObject(await streamTask) as List<Person>;
        }

        public void getShows()
        {
            var endpoint = _db.Parameters.AsQueryable().FirstOrDefault(p => p.ParamName == "SHOWS_ENDPOINT");
            var shows = new List<Show>();
            var url = endpoint.ParamValue;
            var episodes = ShowsAsync(url);
            foreach (var episode in episodes.Result)
                shows.Add(new Show { id = episode._embedded.show.id, name = episode._embedded.show.name });
            _db.Shows.AddRangeAsync(shows);
            _db.SaveChangesAsync();
        }

        public void getCasts()
        {
            var endpoint = _db.Parameters.AsQueryable().FirstOrDefault(p => p.ParamName == "CAST_ENDPOINT");
            var casts = new List<Cast>();
            var showcasts = new List<ShowCast>();
            var shows = _db.Shows.ToList();
            foreach (var show in shows)
            {
                var url = String.Format(endpoint.ParamValue, show.id);
                var people = CastAsync(url);
                foreach (var p in people.Result)
                {
                    casts.Add(new Cast { id = p.person.id, name = p.person.name, birthday = p.person.birthday });
                    showcasts.Add(new ShowCast { CastId = p.person.id, ShowId = show.id });
                }
            }
            _db.Casts.AddRangeAsync(casts);
            _db.ShowCasts.AddRangeAsync(showcasts);
            _db.SaveChangesAsync();
        }

        public List<TvShow> showCast(int page, int pagesize = 25)
        {
            IQueryable<Show> shows = _db.Shows.Skip(page - (1 * pagesize)).Take(pagesize);
            List<TvShow> tvShows = (from show in shows
                                    select new TvShow()
                                    {
                                        id = show.id,
                                        name = show.name,
                                        cast = (from cast in show.ShowCasts
                                                select new TvCast
                                                {
                                                    birthday = cast.Cast.birthday,
                                                    id = cast.Cast.id,
                                                    name = cast.Cast.name
                                                }).ToList()
                                    }).ToList();
            return tvShows;
        }
    }
}
