using Microservicios.Atracciones.Booking.Business.DTOs.Booking;
using Microservicios.Atracciones.Booking.DataAccess.Common;

namespace Microservicios.Atracciones.Booking.Business.Interfaces;

public interface IBookingService
{
    /// <summary>Crea una reserva, valida disponibilidad y calcula el precio final.</summary>
    Task<BookingConfirmationResponse> CreateBookingAsync(Guid userId, CreateBookingRequest request);
    
    /// <summary>Obtiene el detalle completo de una reserva por su código PNR.</summary>
    Task<BookingDetailResponse> GetByPnrAsync(string pnrCode);

    /// <summary>Obtiene el detalle de una reserva por su ID, validando permisos.</summary>
    Task<BookingDetailResponse> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin);

    /// <summary>Historial paginado de reservas de un usuario.</summary>
    Task<PagedResult<BookingSummaryResponse>> GetUserHistoryAsync(Guid userId, int page = 1, int pageSize = 10);

    /// <summary>Búsqueda administrativa de reservas con filtrado por rol.</summary>
    Task<PagedResult<BookingSummaryResponse>> SearchManagementAsync(BookingSearchRequest request, Guid userId, bool isAdmin);
    
    /// <summary>Cancela una reserva si aún está dentro del plazo de cancelación sin costo. Admite bypass de propiedad si es administrador.</summary>
    Task CancelBookingAsync(Guid userId, bool isAdmin, CancelBookingRequest request);

    /// <summary>Consulta el monitor de horarios para una atracción y rango de fechas.</summary>
    Task<List<ScheduleMonitorDto>> GetScheduleMonitorAsync(Guid attractionId, DateOnly from, DateOnly to);

    /// <summary>Genera horarios de disponibilidad en lote a partir de una plantilla.</summary>
    Task<GenerateSchedulesResponse> GenerateSchedulesAsync(GenerateSchedulesRequest request);

    /// <summary>Elimina horarios en lote que no tengan reservas asociadas en un rango de fechas.</summary>
    Task<BulkDeleteSchedulesResponse> BulkDeleteSchedulesAsync(BulkDeleteSchedulesRequest request);
}
