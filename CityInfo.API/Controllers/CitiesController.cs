using CityInfo.API.DataStore;
using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<CityDto>> GetCities()
        {
            return Ok(CitiesDataStore.Current.Cities);            
        }

        [HttpGet("{id}")]
        public ActionResult<CityDto> GetCity([FromRoute] int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == id);
            
            if (city == null)
            { 
                return NotFound();
            } 
            return Ok(city);
        }
    }
}

// ControllerRoutes can be build using Controller name - [Route("api/cities")], but the app will crush if we change the ControllerName
// Hardcoded Name might be better