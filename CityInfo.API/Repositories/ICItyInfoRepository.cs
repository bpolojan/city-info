using CityInfo.API.Entities;
using CityInfo.API.Services;

namespace CityInfo.API.Repositories
{
    public interface ICityInfoRepository
    {
        Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? name, string? searchQuery, int pageNumber, int pageSize);
        Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest);
        Task<bool> CityExistsAsync(int cityId);
        Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId);
        Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId,
            int pointOfInterestId);
        Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest);
        void DeletePointOfInterest(PointOfInterest pointOfInterest);
        Task<bool> SaveChangesAsync();
    }
}

/* *  
 *  Sync
 *  Each request needs a thread. in Sync the call will be blocked until it receives the result.
 *  If we have just 2 Threads, only 2 requests at a time can be handled
 *  IF we have multiple requests and they take 2 log the client will become 503 Service unavailable
 * 
 *  Async - I/O
 *  Using async will free up threads as soon as they can be used for other tasks -> better scalability
 *  Each request needs a thread. But the thread is not blocked, will return to the Thread pool. 
 *  Only when the I/O is finished the thread is requred again to handle the request
 *  Use Threads more efficiently
 *  Same Thread can handle multiple requests
 */


/* *  
    Repository Design Pattern
    Abstraction that reduces Complexity
    Persistence ignorant
 */