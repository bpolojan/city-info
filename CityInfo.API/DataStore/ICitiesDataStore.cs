using CityInfo.API.Models;

namespace CityInfo.API.DataStore
{
    public interface ICitiesDataStore
    {
        List<CityDto> Cities { get; set; }
    }
}