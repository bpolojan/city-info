using AutoMapper;
using CityInfo.API.Entities;
using CityInfo.API.Models;
using CityInfo.API.Repositories;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {
        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService, ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(_cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
        {
            try  // Force 500 Internal Server Error for Demo purposes
            {
                if (!await _cityInfoRepository.CityExistsAsync(cityId))
                {
                    //throw new Exception("Exception sample");
                    _logger.LogInformation($"City {cityId} was not found");
                    return NotFound();
                }

                var pointsOfInterest = await _cityInfoRepository.GetPointsOfInterestForCityAsync(cityId);
                return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterest));
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"City {cityId} was not found", ex);
                return StatusCode(500, "Hei User I know you can see this data. No details for you!");
            }
        }

        [HttpGet("{pointOfInterestId}", Name = "PointOfInterest")]

        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointOfInterest(int cityId, int pointOfInterestId)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                _logger.LogInformation($"City {cityId} was not found");
                return NotFound();
            }

            var pointOfInterestValue = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestValue == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<PointOfInterestDto>(pointOfInterestValue));
        }

        [HttpPost]
        public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest([FromRoute] int cityId, [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
            bool cityExists = await _cityInfoRepository.CityExistsAsync(cityId);
            if (!cityExists)
            {
                return NotFound();
            }

            var finalPointOfInterest = _mapper.Map<PointOfInterest>(pointOfInterest);

            await _cityInfoRepository.AddPointOfInterestForCityAsync(cityId, finalPointOfInterest);
            await _cityInfoRepository.SaveChangesAsync();

            var createdPointOfInterestToReturn = _mapper.Map<PointOfInterestDto>(finalPointOfInterest);

            return CreatedAtRoute("PointOfInterest", new { cityId = cityId, pointOfInterestId = createdPointOfInterestToReturn.Id }, createdPointOfInterestToReturn);
        }

        [HttpPut("{pointofinterestid}")]
        public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId,
           PointOfInterestForUpdateDto pointOfInterest)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(pointOfInterest, pointOfInterestEntity);

            await _cityInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{pointOfInterestId}")]
        public async Task<ActionResult> DeletePointOfIntereest(int cityId, int pointOfInterestId)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);
            _mailService.Send("Point of interest deleted.",
                 $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted.");

            await _cityInfoRepository.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{pointOfInterestId}")]
        public async Task<ActionResult> PartiallyUpdateResource([FromRoute] int cityId, [FromRoute] int pointOfInterestId,
            JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = await _cityInfoRepository.GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            var pointOfInterestToPatch = _mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

            // ModelState as Parameter to get Errors (field might not exist) and Return Errors to User
            // Ex:  "The target location specified by path segment 'invalidproperty' was not found."
            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)// validates only the JsonPatchDocument, but not the pointOfInterestToPatch 
            {
                return BadRequest(ModelState);
            }

            if (!TryValidateModel(pointOfInterestToPatch)) // validates the Model Data Annotations - Ex: Name is Required - can not be removed
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);
            await _cityInfoRepository.SaveChangesAsync();

            return NoContent();
        }

        // Explore the ModelState
        public void ModelStateTestProperties(ModelStateDictionary modelState)
        {
            var modelStateKeys = ModelState.Keys;
            var cityId = ModelState["cityId"];
            var modelStateValues = ModelState.Values;
            var modelStateErrorCount = ModelState.ErrorCount;
            IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
        }
    }
}

/* POST - 201 Created
    - body format is defined by Content-Type Header
    
  
     define Name Of the GetEndPoint
     return Parameters for Calling the Get - cityId, pointOfInterstId
     return the Created Object
     return CreatedAtRoute("PointOfInterest", new { cityId = city.Id, pointOfInterestId = createdPointOfInterest.Id }, createdPointOfInterest);
 */

/* Validation
   - Data Annotations
    Annotations are checked ->

    if (!ModelState.IsValid)    
        return BadRequest();    
    
    ModelState a dictionary that contains:
    - State of Model - collection of properties (key - value)
    - Model Validation - Collection of Error Messages

    
 */

/* PUT - 204 NoContent
   - will fully update the Result
   - If a field value is not provided - the field will be updated to it's default value
 */


/* PATCH - 204 NoContent
 * Json Patch Document
   - list of operations to apply to a resource - to partially update   
   - describes a document structure for expressing a sequence of operations to apply to a Json document

    Available in NewtonsoftJson
Microsoft.AspNetCore.JsonPatch - enable support for JsonPatch
Microsoft.AspNetCore.Mvc.NewtonsoftJson (formatters for Json+ JsonPatch)
    
    Ex:
 [
    {
      "op": "replace",
      "path": "/name",
      "value": "Updated - Central Park"
    },
    {
      "op": "replace",
      "path": "/description",
      "value": "Updated - Description"
    }
 ]
 */