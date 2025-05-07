using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientService
{
    Task<bool> DoesTripExist(int tripId);
    Task<TripDTO> GetTrip(int tripId);
}