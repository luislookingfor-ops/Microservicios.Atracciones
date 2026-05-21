using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microservicios.Atracciones.Booking.DataAccess.Context;

namespace Microservicios.Atracciones.Booking.API.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly AtraccionDbContext _dbContext;

    public HealthController(AtraccionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Verifica la conexión a la base de datos para pruebas rápidas
    /// </summary>
    [HttpGet("db-check")]
    public async Task<IActionResult> CheckDbConnection()
    {
        try
        {
            // Verificamos si podemos abrir una conexión con la base de datos de manera asíncrona
            var canConnect = await _dbContext.Database.CanConnectAsync();
            if (canConnect)
            {
                return Ok(new { status = "success", message = "✅ CONEXION EXITOSA A LA BASE DE DATOS DE SUPABASE!" });
            }
            return StatusCode(500, new { status = "error", message = "❌ No se pudo conectar a la base de datos." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = "error", message = $"❌ ERROR AL CONECTAR: {ex.Message}" });
        }
    }
}
