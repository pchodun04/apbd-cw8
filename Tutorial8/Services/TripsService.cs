using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString =
        "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=APBD;Integrated Security=True;";

    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new List<TripDTO>();

        string command = "SELECT IdTrip, Name, Description, DateFrom, DateTo, MaxPeople FROM Trip";
        string commandTrip =
            "SELECT c.IdCountry, c.Name FROM Country c JOIN Country_Trip ct ON c.IdCountry = ct.IdCountry WHERE ct.IdTrip = @IdTrip";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd1 = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd1.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idOrdinal = reader.GetOrdinal("IdTrip");
                    trips.Add(new TripDTO()
                    {
                        Id = reader.GetInt32(idOrdinal),
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        DateFrom = reader.GetDateTime(3),
                        DateTo = reader.GetDateTime(4),
                        MaxPeople = reader.GetInt32(5),
                        Countries = new List<CountryDTO>()
                    });
                }
            }

            foreach (var trip in trips)
            {
                using (SqlCommand cmd2 = new SqlCommand(commandTrip, conn))
                {
                    cmd2.Parameters.AddWithValue("@IdTrip", trip.Id);
                    //await cmd.ExecuteNonQueryAsync();
                    using (var readerCountry = await cmd2.ExecuteReaderAsync())
                    {
                        while (await readerCountry.ReadAsync())
                        {
                            trip.Countries.Add(new CountryDTO()
                            {
                                IdCountry = readerCountry.GetInt32(0),
                                Name = readerCountry.GetString(1)
                            });
                        }
                    }
                }
            }
        }

        return trips;
    }

    public async Task<bool> DoesTripExist(int tripId)
    {
        string command = "SELECT 1 FROM Trip WHERE IdTrip = @IdTrip";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@IdTrip", tripId);
            await conn.OpenAsync();

            return await cmd.ExecuteScalarAsync() != null;
        }
    }

    public async Task<List<ClientTripDTO>> GetTrip(int clientId)
    {
        var trips = new List<ClientTripDTO>();

        string clientTripsQuery = @"
        SELECT 
            t.IdTrip, 
            t.Name, 
            t.Description, 
            t.DateFrom, 
            t.DateTo, 
            t.MaxPeople,
            ct.RegisteredAt,
            ct.PaymentDate
        FROM Trip t
        JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
        WHERE ct.IdClient = @clientId
        ORDER BY ct.RegisteredAt DESC";

        string countriesQuery = @"
        SELECT c.IdCountry, c.Name 
        FROM Country c
        JOIN Country_Trip ct ON c.IdCountry = ct.IdCountry
        WHERE ct.IdTrip = @TripId";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (SqlCommand command = new SqlCommand(clientTripsQuery, connection))
            {
                command.Parameters.AddWithValue("@ClientId", clientId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        trips.Add(new ClientTripDTO()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("IdTrip")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                            DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                            MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                            RegisteredAt = reader.GetInt32(reader.GetOrdinal("RegisteredAt")),
                            PaymentDate = await reader.IsDBNullAsync(reader.GetOrdinal("PaymentDate"))
                                ? null
                                : reader.GetInt32(reader.GetOrdinal("PaymentDate")),
                            Countries = new List<CountryDTO2>()
                        });
                    }
                }
            }

            foreach (var trip in trips)
            {
                using (SqlCommand countryCommand = new SqlCommand(countriesQuery, connection))
                {
                    countryCommand.Parameters.AddWithValue("@TripId", trip.Id);

                    using (var countryReader = await countryCommand.ExecuteReaderAsync())
                    {
                        while (await countryReader.ReadAsync())
                        {
                            trip.Countries.Add(new CountryDTO2()
                            {
                                IdCountry = countryReader.GetInt32(countryReader.GetOrdinal("IdCountry")),
                                Name = countryReader.GetString(countryReader.GetOrdinal("Name"))
                            });
                        }
                    }
                }
            }
        }

        return trips;
    }
}