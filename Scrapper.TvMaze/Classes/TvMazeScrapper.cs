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
        public TvMazeScrapper()
        {
        }

        public async Task<List<Episode>> ShowsAsync(string endPoint)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
                var serializer = new DataContractJsonSerializer(typeof(List<Episode>));
                var streamTask = client.GetStreamAsync(endPoint);
                return serializer.ReadObject(await streamTask) as List<Episode>;
            }
            catch (Exception ex)
            {
                //Handle exceptions
                return new List<Episode>();
            }
        }
        public async Task<List<Person>> CastAsync(string endPoint)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");
                var serializer = new DataContractJsonSerializer(typeof(List<Person>));
                var streamTask = client.GetStreamAsync(endPoint);
                return serializer.ReadObject(await streamTask) as List<Person>;
            }
            catch (Exception ex)
            {
                //Handle exceptions
                return new List<Person>();
            }
        }

        public void getShows()
        {
            try
            {
                ScrapperContext _db = new ScrapperContext();
                var endpoint = _db.Parameters.AsQueryable().FirstOrDefault(p => p.ParamName == "SHOWS_ENDPOINT");
                var shows = new List<Show>();
                var url = endpoint.ParamValue;
                var episodes = ShowsAsync(url);
                foreach (var episode in episodes.Result)
                {
                    if (!shows.Any(p => p.id == episode._embedded.show.id))
                        shows.Add(new Show { id = episode._embedded.show.id, name = episode._embedded.show.name });
                }
                _db.Shows.AddRange(shows);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                //Handle exceptions
            }
        }

        public bool refreshData()
        {
            ScrapperContext _db = new ScrapperContext();
            var _update = _db.Parameters.AsQueryable().FirstOrDefault(p => p.ParamName == "LAST_RUN");
            return DateTime.Now.Subtract(Convert.ToDateTime(_update.ParamValue)).Days > 0;
        }

        public void updateLastRun(DateTime lastRun)
        {
            ScrapperContext _db = new ScrapperContext();
            var _update = _db.Parameters.AsQueryable().FirstOrDefault(p => p.ParamName == "LAST_RUN");
            _update.ParamValue = lastRun.ToShortDateString();
            _db.SaveChanges();
        }

        public void getCasts()
        {
            try
            {
                ScrapperContext _db = new ScrapperContext();
                var endpoint = _db.Parameters.AsQueryable().FirstOrDefault(p => p.ParamName == "CAST_ENDPOINT");
                //var casts = new List<Cast>();
                //var showcasts = new List<ShowCast>();
                var shows = _db.Shows.ToList();
                foreach (var show in shows)
                {
                    var url = String.Format(endpoint.ParamValue, show.id);
                    var people = CastAsync(url);
                    foreach (var p in people.Result)
                    {
                        if (!_db.Casts.Any(x => x.id == p.person.id))
                        {
                            _db.Casts.Add(new Cast { id = p.person.id, name = p.person.name, birthday = p.person.birthday });
                        }
                        _db.ShowCasts.Add(new ShowCast { CastId = p.person.id, ShowId = show.id });
                        _db.SaveChanges();
                    }
                }
            }
            catch(Exception ex)
            {
                //Handle exceptions
            }
        }

        public List<TvShow> showCast(int page, int pagesize = 25)
        {
            try
            {
                ScrapperContext _db = new ScrapperContext();
                IQueryable<Show> shows = _db.Shows.Skip((page - 1) * pagesize).Take(pagesize);
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
            catch (Exception ex)
            {
                //Handle exceptions
                return new List<TvShow>();
            }
        }
    }
}
