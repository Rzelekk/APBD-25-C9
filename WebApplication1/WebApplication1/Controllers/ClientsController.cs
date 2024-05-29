using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ApbdContext _context;

    public ClientsController(ApbdContext context)
    {
        _context = context;
    }

    [HttpDelete]
    [Route("{idClient}")]
    public async Task<IActionResult> Delete(int idClient)
    {
        var totalTrips = await _context.ClientTrips.CountAsync(e => e.IdClient == idClient);
        if (totalTrips > 0)
        {
            return Conflict($"This client id: {idClient} has booked trips");
        }
        
        var clientToDelate = await _context.Clients.FindAsync(idClient);

        if (clientToDelate != null)
        {
            _context.Clients.Remove(clientToDelate);
            await _context.SaveChangesAsync();
        }
        else
        {
            return NotFound($"Client with given id: {idClient} does not exist");
        }

        return NoContent();
    }
}