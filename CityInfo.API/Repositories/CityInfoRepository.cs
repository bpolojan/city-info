using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using CityInfo.API.Services;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Repositories
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private readonly CityInfoContext _context;


        public CityInfoRepository(CityInfoContext context)
        {
            _context = context ?? throw new ArgumentException(nameof(context));
        }

        public async Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? name, string? searchQuery, int pageNumber, int pageSize)
        {
            IQueryable<City> query = _context.Cities;

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name == name.Trim());
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(x => x.Name.Contains(searchQuery) || (x.Description != null && x.Description.Contains(searchQuery)));
            }

            var totalItemsCount = await query.CountAsync();
            var paginationMetaData = new PaginationMetadata(totalItemsCount, pageSize, pageNumber);

            // Pagination
            var collectionToReturn =  await query
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return (collectionToReturn, paginationMetaData);
        }

        public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
        {
            if (includePointsOfInterest)
            {
                return await _context.Cities.Include(c => c.PointsOfInterest)
                    .Where(c => c.Id == cityId).FirstOrDefaultAsync();
            }

            return await _context.Cities
                  .Where(c => c.Id == cityId).FirstOrDefaultAsync();
        }

        public async Task<bool> CityExistsAsync(int cityId)
        {
            return await _context.Cities.AnyAsync(c => c.Id == cityId);
        }

        public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(
            int cityId,
            int pointOfInterestId)
        {
            return await _context.PointsOfInterest
               .Where(p => p.CityId == cityId && p.Id == pointOfInterestId)
               .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(
            int cityId)
        {
            return await _context.PointsOfInterest
                           .Where(p => p.CityId == cityId).ToListAsync();
        }

        public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
        {
            var city = await GetCityAsync(cityId, false);
            if (city != null)
            {
                city.PointsOfInterest.Add(pointOfInterest);
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            int countOfChanges = await _context.SaveChangesAsync();
            return (countOfChanges >= 0);
        }

        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            _context.PointsOfInterest.Remove(pointOfInterest);
        }

        public async Task<bool> CityNameMatchesCityId(string? cityName, int cityId)
        {
            var plm = await _context.Cities.AnyAsync(c => c.Id == cityId && c.Name == cityName);
            return plm;
        }
    }
}