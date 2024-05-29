using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models.DTO;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripController : ControllerBase
{
    private readonly ApbdContext _context;

    public TripController(ApbdContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetTrips(int page = 1, int pageSize = 10)
    {

        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var totalTrips = await _context.Trips.CountAsync();
        var allPages = (int)Math.Ceiling(totalTrips / (double)pageSize);
        
        
        var trips = await _context.Trips
                // ile elementów ma pominąć
            .Skip((page-1)*pageSize)
                //pobiera pierwsze n elementów po tym pominięciu przez skipa
            .Take(pageSize)
            .Select(e => new
        {   
            Name = e.Name,
            Description = e.Description,
            DateFrom = e.DateFrom,
            DateTo = e.DateTo,
            MaxPeople = e.MaxPeople,
            Countries = e.IdCountries.Select(c => new
            {
                Name = c.Name
            }),
            
            Clients = e.ClientTrips.Select(ct => new 
                {FirstName = ct.IdClientNavigation.FirstName, LastName = ct.IdClientNavigation.LastName})
        }).OrderByDescending(e => e.DateFrom).ToListAsync();

        var response = new
        {
            pageNum = page,
            pageSize = pageSize,
            allPages = allPages,
            trips = trips
        };
        
        return Ok(response);
    }

    [HttpPost]
    [Route("{idTrip}/clients")]
    public async Task<IActionResult> AddClientToTrip(int idTrip, [FromBody] ClientDTO clientDto)
    {

        var now = DateTime.Now;
        // Sprawdzenie czy wycieczka istnieje
        var trip = await _context.Trips.FindAsync(idTrip);
        if (trip == null)
        {
            return NotFound(new { error = "Trip does not exist" });
        }
        
        // Sprawdzenie czy data wycieczki jest w przyszłości
        if (trip.DateFrom < DateTime.Today)
        {
            return BadRequest(new { error = "Cannot register for a past trip" });
        }
        
        // Sprawdzenie czy klient istnieje w bazie
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Pesel == clientDto.Pesel);
        
        if (client == null)
        {
           var  newClient = new Client
            {
                FirstName = clientDto.FirstName,
                LastName = clientDto.LastName,
                Email = clientDto.Email,
                Telephone = clientDto.Telephone,
                Pesel = clientDto.Pesel
            };
            await _context.Clients.AddAsync(newClient);
            await _context.SaveChangesAsync();

            client = newClient;
        }
        
        // Sprawdzenie czy klient jest już zapisany na tę wycieczkę
        var existingRegistration = await _context.ClientTrips
            .FirstOrDefaultAsync(ct => ct.IdClient == client.IdClient && ct.IdTrip == idTrip);
        
        if (existingRegistration != null)
        {
            return BadRequest(new { error = "Client already registered for this trip" });
        }

        var clientTrip = new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = idTrip,
            RegisteredAt = now,
            PaymentDate = clientDto.PaymentDate
        };

        await _context.ClientTrips.AddAsync(clientTrip);
        await _context.SaveChangesAsync();
    
        return Ok("Klient został przpisany do wycieczki");
    }
}