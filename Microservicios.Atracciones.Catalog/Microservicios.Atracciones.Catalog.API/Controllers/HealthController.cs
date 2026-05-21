using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;
using System.Threading.Tasks;

namespace Microservicios.Atracciones.Catalog.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HealthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("check-db")]
        public async Task<IActionResult> CheckDatabaseConnection()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                return StatusCode(500, new { Status = "Error", Message = "Connection string 'DefaultConnection' is not configured." });
            }

            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                if (connection.State == ConnectionState.Open)
                {
                    return Ok(new { Status = "Success", Message = "Successfully connected to the database." });
                }
                
                return StatusCode(500, new { Status = "Error", Message = "Failed to open connection." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { Status = "Error", Message = $"Failed to connect to the database. Exception: {ex.Message}" });
            }
        }
    }
}
