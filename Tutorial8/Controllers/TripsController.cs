using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripsService _tripsService;
        //private readonly IClientService _clientService;

        public TripsController(ITripsService tripsService)
        {
            _tripsService = tripsService;
            //_clientService = clientService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var trips = await _tripsService.GetTrips();
            return Ok(trips);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrip(int id)
        {
            if( !await _tripsService.DoesTripExist(id)){
             return NotFound();
            }
            var trip = await _tripsService.GetTrip(id);
            return Ok(trip);
        }
    }
}
