using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestApp.Models;

namespace TestApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherInfoController : ControllerBase
    {
        private ApplicationContext db;
        private static List<WeatherInfoCurrent> cache = new List<WeatherInfoCurrent>(); 
        public WeatherInfoController(ApplicationContext context)
        {
            db = context;
            //initializing db 
            if (!db.WeatherInfos.Any())
            {
                db.WeatherInfos.Add(new WeatherInfo
                {
                    ID = 1,
                    CityName = "Kharkov",
                    CurrentTemperature = 35,
                    TimeStamp = DateTime.Now,
                    IsArchived = false
                });
                db.WeatherInfos.Add(new WeatherInfo
                {
                    ID = 2,
                    CityName = "Kiev",
                    CurrentTemperature = 32,
                    TimeStamp = DateTime.Now,
                    IsArchived = false
                });
                db.WeatherInfos.Add(new WeatherInfo
                {
                    ID = 3,
                    CityName = "Lvov",
                    CurrentTemperature = 33,
                    TimeStamp = DateTime.Now,
                    IsArchived = false
                });

                //multiple Kharkov object for min/avg/max with different timestamp
                db.WeatherInfos.Add(new WeatherInfo
                {
                    ID = 4,
                    CityName = "Kharkov",
                    CurrentTemperature = 34,
                    TimeStamp = DateTime.Now.AddDays(-1),
                    IsArchived = false
                });
                db.WeatherInfos.Add(new WeatherInfo
                {
                    ID = 5,
                    CityName = "Kharkov",
                    CurrentTemperature = 31,
                    TimeStamp = DateTime.Now.AddDays(-2),
                    IsArchived = false
                });
                db.WeatherInfos.Add(new WeatherInfo
                {
                    ID = 6,
                    CityName = "Kharkov",
                    CurrentTemperature = 28,
                    TimeStamp = DateTime.Now.AddDays(-3),
                    IsArchived = false
                });

                db.SaveChanges();
            }
        }
        //kharkov/history
        [HttpGet("{name}/history")]
        public async Task<ActionResult<IEnumerable<WeatherInfo>>> WeatherConditionHistoryByCityName(string name)
        {
            return await db.WeatherInfos.Where(x => x.CityName == name && !x.IsArchived).ToListAsync();
        }

        //Kharkov, Kiev, Lvov
        [HttpGet("{name}")]
        public async Task<ActionResult<WeatherInfoCurrent>> WeatherConditionByCityName(string name)
        {
            if (cache.Any(x => x.CityName == name))
            {
                return cache.FirstOrDefault(x => x.CityName == name);
            }

            var cityWeather = db.WeatherInfos.Where(x => x.CityName == name && !x.IsArchived);
            WeatherInfo wi = await cityWeather.OrderBy(x => x.TimeStamp).LastOrDefaultAsync();
            if (wi == null)
                return NotFound();

            var maxTemp = await cityWeather.MaxAsync(x => x.CurrentTemperature);
            var minTemp = await cityWeather.MinAsync(x => x.CurrentTemperature);
            var avgTemp = await cityWeather.AverageAsync(x => x.CurrentTemperature);

            var result = new WeatherInfoCurrent
            {
                ID = wi.ID,
                CityName = wi.CityName,
                CurTemperature = wi.CurrentTemperature,
                TimeStamp = wi.TimeStamp,
                MaxTemperature = maxTemp,
                MinTemparature = minTemp,
                AvgTemperature = avgTemp
            };

            cache.Add(result);

            return new OkObjectResult(result);
        }

        [HttpPost]
        public async Task<ActionResult<WeatherInfo>> AddWeather(WeatherInfo weatherInfo)
        {
            if (weatherInfo == null)
            {
                return BadRequest();
            }
            weatherInfo.TimeStamp = DateTime.Now;

            db.WeatherInfos.Add(weatherInfo);
            await db.SaveChangesAsync();

            cache.RemoveAll(x => x.CityName == weatherInfo.CityName);

            return CreatedAtAction(
                nameof(AddWeather),
                new { ID = weatherInfo.ID },
                weatherInfo);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<WeatherInfo>> UpdateWeather(int id, WeatherInfo weatherInfo)
        {
            if (weatherInfo == null)
            {
                return BadRequest();
            }

            var wi = await db.WeatherInfos.FindAsync(id);
            if (wi == null)
            {
                return NotFound();
            }

            wi.CityName = weatherInfo.CityName;
            wi.CurrentTemperature = weatherInfo.CurrentTemperature;
            wi.TimeStamp = weatherInfo.TimeStamp;
            wi.IsArchived = weatherInfo.IsArchived;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!db.WeatherInfos.Any(e => e.ID == id))
            {
                return NotFound();
            }
            cache.RemoveAll(x => x.CityName == weatherInfo.CityName);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> ArchiveWeatherInfo(int id)
        {
            var wi = await db.WeatherInfos.FindAsync(id);

            if (wi == null)
            {
                return NotFound();
            }

            wi.IsArchived = true; //imitation of delete method
            await db.SaveChangesAsync();
            cache.RemoveAll(x => x.CityName == wi.CityName);

            return NoContent();
        }


    }

}

