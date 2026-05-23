using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microservicios.Atracciones.Booking.DataAccess.Repositories.Interfaces;
using Microservicios.Atracciones.Booking.DataManagement.Interfaces;
using Microservicios.Atracciones.Booking.DataManagement.Models;

namespace Microservicios.Atracciones.Booking.DataManagement.Services;

public class InventoryDataService : IInventoryDataService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InventoryDataService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AvailabilitySlotNode>> GetAvailabilityAsync(Guid attractionId, DateTime startDate, DateTime endDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var start = DateOnly.FromDateTime(startDate);
        if (start < today) start = today;

        var end = DateOnly.FromDateTime(endDate);

        // En el microservicio de Booking, AvailabilitySlot tiene ProductId o AttractionId
        // En este caso, según el script original, tiene product_id.
        var slots = await _unitOfWork.AvailabilitySlots.Query()
            .Where(s => s.ProductId == attractionId // Asumiendo ProductId para simplificar o filtrar por producto
                        && s.SlotDate >= start 
                        && s.SlotDate <= end
                        && s.CapacityAvailable > 0
                        && s.IsActive)
            .OrderBy(s => s.SlotDate).ThenBy(s => s.StartTime)
            .ToListAsync();

        return _mapper.Map<IEnumerable<AvailabilitySlotNode>>(slots).ToList();
    }

    public async Task<AvailabilitySlotNode?> GetSlotByIdAsync(Guid slotId)
    {
        var slot = await _unitOfWork.AvailabilitySlots.Query()
            .FirstOrDefaultAsync(s => s.Id == slotId && s.IsActive);

        return slot == null ? null : _mapper.Map<AvailabilitySlotNode>(slot);
    }

    public async Task<bool> DecrementSlotCapacityAsync(Guid slotId, short quantity)
    {
        var slot = await _unitOfWork.AvailabilitySlots.Query()
            .FirstOrDefaultAsync(s => s.Id == slotId);

        if (slot == null || (quantity > 0 && slot.CapacityAvailable < quantity))
            return false;

        slot.CapacityAvailable -= quantity;
        slot.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.AvailabilitySlots.Update(slot);

        return await _unitOfWork.CompleteAsync() > 0;
    }

    public async Task<IEnumerable<DataAccess.Entities.AvailabilitySlot>> GetSlotsForMonitorAsync(Guid attractionId, DateOnly from, DateOnly to)
    {
        return await _unitOfWork.AvailabilitySlots.Query(asNoTracking: true)
            .Where(s => s.ProductId == attractionId
                        && s.SlotDate >= from
                        && s.SlotDate <= to)
            .OrderBy(s => s.SlotDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<int> GenerateSlotsAsync(Guid attractionId, DateOnly from, DateOnly to, List<TimeOnly> times, List<int> weekDays, short capacity)
    {
        var existingSlots = await _unitOfWork.AvailabilitySlots.Query(asNoTracking: true)
            .Where(s => s.ProductId == attractionId
                        && s.SlotDate >= from
                        && s.SlotDate <= to)
            .Select(s => new { s.SlotDate, s.StartTime })
            .ToListAsync();

        var existingSet = existingSlots
            .Select(s => (s.SlotDate, s.StartTime))
            .ToHashSet();

        var newSlots = new List<DataAccess.Entities.AvailabilitySlot>();

        var currentDate = from;
        while (currentDate <= to)
        {
            int dayOfWeekInt = (int)currentDate.DayOfWeek;
            
            if (weekDays.Contains(dayOfWeekInt))
            {
                foreach (var time in times)
                {
                    if (!existingSet.Contains((currentDate, time)))
                    {
                        newSlots.Add(new DataAccess.Entities.AvailabilitySlot
                        {
                            Id = Guid.NewGuid(),
                            ProductId = attractionId,
                            SlotDate = currentDate,
                            StartTime = time,
                            EndTime = time.AddHours(2),
                            CapacityTotal = capacity,
                            CapacityAvailable = capacity,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
            currentDate = currentDate.AddDays(1);
        }

        if (newSlots.Any())
        {
            await _unitOfWork.AvailabilitySlots.AddRangeAsync(newSlots);
            await _unitOfWork.CompleteAsync();
        }

        return newSlots.Count;
    }

    public async Task<int> BulkDeleteSlotsAsync(Guid attractionId, DateOnly from, DateOnly to)
    {
        var slotsToDelete = await _unitOfWork.AvailabilitySlots.Query(asNoTracking: false)
            .Where(s => s.ProductId == attractionId
                        && s.SlotDate >= from
                        && s.SlotDate <= to
                        && !s.Bookings.Any())
            .ToListAsync();

        if (slotsToDelete.Any())
        {
            _unitOfWork.AvailabilitySlots.DeleteRange(slotsToDelete);
            await _unitOfWork.CompleteAsync();
        }

        return slotsToDelete.Count;
    }
}
