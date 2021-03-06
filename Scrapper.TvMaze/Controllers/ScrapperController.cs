﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Scrapper.TvMaze.Models;
using Scrapper.TvMaze.Classes;
using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Scrapper.TvMaze.Controllers
{
    [Route("api/[controller]")]
    public class ScrapperController : Controller
    {
        // GET api/<controller>/5
        [HttpGet("{page}")]
        public ActionResult<List<TvShow>> GetShows(int page)
        {
            var scrapper = new TvMazeScrapper();
            if (scrapper.refreshData())
            {
                scrapper.getShows();
                scrapper.getCasts();
                scrapper.updateLastRun(DateTime.Now);
            }
            return scrapper.showCast(page);
        }
    }
}
