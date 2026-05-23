using Microservicios.Atracciones.Booking.DataManagement.Models;

namespace Microservicios.Atracciones.Booking.DataManagement.Interfaces;

public interface IInventoryDataService
{
    /// <summary>
    /// Consulta slots de disponibilidad para una atracción en un rango de fechas.
    /// </summary>
    Task<IEnumerable<AvailabilitySlotNode>> GetAvailabilityAsync(Guid attractionId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Obtiene un slot específico por su ID.
    /// </summary>
    Task<AvailabilitySlotNode?> GetSlotByIdAsync(Guid slotId);

    /// <summary>
    /// Decrementa la capacidad disponible de un slot. Admite valores negativos para incrementar.
    /// </summary>
    Task<bool> DecrementSlotCapacityAsync(Guid slotId, short quantity);

    /// <summary>
    /// Consulta todos los slots de disponibilidad (incluso los agotados o inactivos) en un rango para el monitor.
    /// </summary>
    Task<IEnumerable<Microservicios.Atracciones.Booking.DataAccess.Entities.AvailabilitySlot>> GetSlotsForMonitorAsync(Guid attractionId, DateOnly from, DateOnly to);

    /// <summary>
    /// Genera de forma masiva los slots de disponibilidad basados en un rango, días de la semana y horas de salida.
    /// </summary>
    Task<int> GenerateSlotsAsync(Guid attractionId, DateOnly from, DateOnly to, List<TimeOnly> times, List<int> weekDays, short capacity);

    /// <summary>
    /// Elimina de forma masiva los slots de disponibilidad sin reservas en un rango de fechas.
    /// </summary>
    Task<int> BulkDeleteSlotsAsync(Guid attractionId, DateOnly from, DateOnly to);
}
