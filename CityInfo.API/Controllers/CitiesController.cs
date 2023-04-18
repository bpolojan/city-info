using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;
        const int maxCitiesPageSize = 20;

        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(_cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet] //Filter Search Pagination
        public async Task<ActionResult<IEnumerable<CityDto>>> GetCities([FromQuery(Name = "filterbyname")] string? name, string? searchQuery, 
            int pageNumber = 1, int pageSize = 10)
        {
            if (pageSize > maxCitiesPageSize)
            {
                pageSize = maxCitiesPageSize;
            }

            var (cityEntities, paginationMetadata) = await _cityInfoRepository.GetCitiesAsync(name, searchQuery, pageNumber, pageSize);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cityEntities));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetCity([FromRoute] int id, [FromQuery] bool includePointsOfInterest = false)
        {
            var city = await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest);

            if (city == null)
            {
                return NotFound();
            }
            if (includePointsOfInterest)
            {
                return Ok(_mapper.Map<CityDto>(city));
            }
            return Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(city));
        }
    }
}

/* ControllerRoutes
    - can be build using Controller name - [Route("api/cities")], but the app will crush if we change the ControllerName
    - Hardcoded Name might be better
*/

/* Binding Source Attributes - where to find the binding values
    [FromBody]
    [FromRoute]
    [FromQuery]
    [FromHead]
*/

/* Paging - will improve performance
 
 * Add Pagination Data in Headers   
   Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));
*/


/* Deffered Execution
    Create just the definition of the Query -> IQuerable<T> document
    Execution is deferred until the query is iterated over
    ToList(), ToDictionary()
*/