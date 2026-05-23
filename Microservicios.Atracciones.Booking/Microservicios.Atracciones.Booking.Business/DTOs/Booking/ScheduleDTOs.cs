using System.ComponentModel.DataAnnotations;

namespace Microservicios.Atracciones.Booking.Business.DTOs.Booking;

public class ScheduleMonitorDto
{
    public Guid Id { get; set; }
    public Guid AttractionId { get; set; }
    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public short CapacityTotal { get; set; }
    public short CapacityAvailable { get; set; }
    public short CapacitySold => (short)(CapacityTotal - CapacityAvailable);
}

public class GenerateSchedulesRequest
{
    [Required]
    public Guid AttractionId { get; set; }
    [Required]
    public DateOnly DateFrom { get; set; }
    [Required]
    public DateOnly DateTo { get; set; }
    [Required]
    public List<string> Times { get; set; } = [];
    [Required]
    public List<int> WeekDays { get; set; } = [];
    [Required]
    public short CapacityPerSlot { get; set; }
}

public class GenerateSchedulesResponse
{
    public int Count { get; set; }
}

public class BulkDeleteSchedulesRequest
{
    [Required]
    public Guid AttractionId { get; set; }
    [Required]
    public DateOnly From { get; set; }
    [Required]
    public DateOnly To { get; set; }
}

public class BulkDeleteSchedulesResponse
{
    public int Deleted { get; set; }
}
