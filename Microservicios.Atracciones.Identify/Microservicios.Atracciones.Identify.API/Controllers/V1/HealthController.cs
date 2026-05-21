using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microservicios.Atracciones.Identify.DataAccess.Context;

namespace Microservicios.Atracciones.Identify.API.Controllers.V1;

[ApiController]
[Route("api/v1/corrales-jorge/health")]
public class HealthController : ControllerBase
{
    private readonly AtraccionDbContext _context;

    public HealthController(AtraccionDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Verifica la conexión con la base de datos PostgreSQL en Supabase.
    /// </summary>
    [HttpGet("db-check")]
    public async Task<IActionResult> CheckDatabaseConnection()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            if (canConnect)
            {
                return Ok(new { status = "success", message = "Successfully connected to the Supabase database." });
            }
            return StatusCode(500, new { status = "error", message = "Could not connect to the database." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = "error", message = "Error checking database connection.", details = ex.Message });
        }
    }
}
