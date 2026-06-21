using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microservicios.Atracciones.Gateway.Hubs;
using System.Threading.Tasks;

namespace Microservicios.Atracciones.Gateway.Controllers
{
    [ApiController]
    [Route("api/v1/corrales-jorge/notify")]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("attraction-created")]
        public async Task<IActionResult> NotifyAttractionCreated([FromBody] object attractionData)
        {
            await _hubContext.Clients.All.SendAsync("OnAttractionCreated", attractionData);
            return Ok(new { success = true, message = "Notificación de creación enviada a WebSockets." });
        }

        [HttpPost("attraction-updated")]
        public async Task<IActionResult> NotifyAttractionUpdated([FromBody] object attractionData)
        {
            await _hubContext.Clients.All.SendAsync("OnAttractionUpdated", attractionData);
            return Ok(new { success = true, message = "Notificación de actualización enviada a WebSockets." });
        }

        [HttpPost("attraction-deleted")]
        public async Task<IActionResult> NotifyAttractionDeleted([FromBody] object attractionData)
        {
            await _hubContext.Clients.All.SendAsync("OnAttractionDeleted", attractionData);
            return Ok(new { success = true, message = "Notificación de eliminación enviada a WebSockets." });
        }

        [HttpPost("booking-created")]
        public async Task<IActionResult> NotifyBookingCreated([FromBody] object bookingData)
        {
            await _hubContext.Clients.All.SendAsync("OnBookingCreated", bookingData);
            return Ok(new { success = true, message = "Notificación de reserva creada enviada a WebSockets." });
        }

        [HttpPost("availability-updated")]
        public async Task<IActionResult> NotifyAvailabilityUpdated([FromBody] object availabilityData)
        {
            await _hubContext.Clients.All.SendAsync("OnAvailabilityUpdated", availabilityData);
            return Ok(new { success = true, message = "Notificación de cupos actualizada enviada a WebSockets." });
        }
    }
}
