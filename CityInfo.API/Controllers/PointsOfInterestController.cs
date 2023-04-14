using CityInfo.API.DataStore;
using CityInfo.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
        {
            var city  = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null) 
            { 
                return NotFound();
            }
            return Ok(city.PointsOfInterest);
        }

        [HttpGet("{pointOfInterestId}")]
        public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId,int pointOfInterestId)
        {
            var pointOfInterestValue = CitiesDataStore.Current.Cities.FirstOrDefault(x => x.Id == cityId)?.PointsOfInterest.FirstOrDefault(x => x.Id == pointOfInterestId);
            if (pointOfInterestValue == null)
            {
                return NotFound();
            }
            return Ok(pointOfInterestValue);
        }
    }
}