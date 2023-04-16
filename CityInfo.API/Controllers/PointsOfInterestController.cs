using CityInfo.API.DataStore;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Http;
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
        private readonly ICitiesDataStore _citiesDataStore;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService, ICitiesDataStore citiesDataStore)
        {
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService?? throw new ArgumentNullException(nameof(mailService));
            _citiesDataStore = citiesDataStore ?? throw new ArgumentNullException(nameof(citiesDataStore)); ;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
        {
            try
            {
                var city = _citiesDataStore.Cities.FirstOrDefault(x => x.Id == cityId);
                if (city == null)
                {
                    //throw new Exception("Exception sample");
                    _logger.LogInformation($"City {cityId} was not found");
                    return NotFound();
                }
                return Ok(city.PointsOfInterest);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"City { cityId} was not found", ex);
                return StatusCode(500, "Hei User I know you can see this data. No details for you!");
            }            
        }

        [HttpGet("{pointOfInterestId}", Name = "PointOfInterest")]

        public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId, int pointOfInterestId)
        {
            var pointOfInterestValue = _citiesDataStore.Cities.FirstOrDefault(x => x.Id == cityId)?.PointsOfInterest.FirstOrDefault(x => x.Id == pointOfInterestId);
            if (pointOfInterestValue == null)
            {
                return NotFound();
            }
            return Ok(pointOfInterestValue);
        }

        [HttpPost]
        public ActionResult<PointOfInterestDto> CreatePointOfInterest([FromRoute] int cityId, [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
            var city = _citiesDataStore.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            city.PointsOfInterest.Add(new PointOfInterestDto() { Id = 200, Name = pointOfInterest.Name, Description = pointOfInterest.Description });
            var createdPointOfInterest = city.PointsOfInterest.FirstOrDefault(x => x.Name == pointOfInterest.Name);

            return CreatedAtRoute("PointOfInterest", new { cityId = city.Id, pointOfInterestId = createdPointOfInterest.Id }, createdPointOfInterest);
        }

        [HttpPut("{pointOfInterestId}")]
        public ActionResult UpdatePointOfInterest([FromRoute] int cityId, [FromRoute] int pointOfInterestId, [FromBody] PointOfInterestForUpdateDto pointOfInterest)
        {
            var city = _citiesDataStore.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestToUpdate = city.PointsOfInterest.FirstOrDefault(x => x.Id == pointOfInterestId);
            if (pointOfInterestToUpdate == null)
            {
                return NotFound();
            }
            pointOfInterestToUpdate.Name = pointOfInterest.Name;
            pointOfInterestToUpdate.Description = pointOfInterest.Description;

            return NoContent();
        }

        [HttpDelete("{pointOfInterestId}")]
        public ActionResult DeletePointOfIntereest(int cityId, int pointOfInterestId)
        {
            var city = _citiesDataStore.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(x => x.Id == pointOfInterestId);
            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }
            city.PointsOfInterest.Remove(pointOfInterestFromStore);
            _mailService.Send("Point of interest deleted.",
                 $"Point of interest {pointOfInterestFromStore.Name} with id {pointOfInterestFromStore.Id} was deleted.");
            return NoContent(); 
        }

        [HttpPatch("{pointOfInterestId}")]
        public ActionResult PartiallyUpdateResource([FromRoute] int cityId, [FromRoute] int pointOfInterestId,
            JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            var city = _citiesDataStore.Cities.FirstOrDefault(x => x.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(x => x.Id == pointOfInterestId);
            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            var pointOfInterestToPatch = new PointOfInterestForUpdateDto()
            {
                Name = pointOfInterestFromStore.Name,
                Description = pointOfInterestFromStore.Description
            };

            // ModelState as Parameter to get errors (field might not exist) and Return Errors to User
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

            pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
            pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;

            return NoContent();
        }

        public void ModelStateTestProperties(ModelStateDictionary modelState )
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