using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microservicios.Atracciones.Catalog.Business.DTOs.Attraction;
using Microservicios.Atracciones.Catalog.Business.Interfaces;
using Microservicios.Atracciones.Catalog.DataAccess.Common;
using System.Security.Claims;

namespace Microservicios.Atracciones.Catalog.API.Controllers.V1;

[ApiController]
[Route("api/v1/corrales-jorge/attraction")]
public class AttractionController : ControllerBase
{
    private readonly IAttractionService _attractionService;

    public AttractionController(IAttractionService attractionService)
    {
        _attractionService = attractionService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<AttractionSummaryResponse>>> Search([FromQuery] AttractionSearchRequest request)
    {
        var result = await _attractionService.SearchAsync(request);
        return Ok(result);
    }

    [HttpGet("management")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<AttractionSummaryResponse>>> SearchManagement([FromQuery] AttractionSearchRequest request)
    {
        var currentUserId = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : Guid.Empty;
        bool isAdmin = User.IsInRole("Admin") || currentUserId == Guid.Empty; // sin token = admin

        var result = await _attractionService.SearchManagementAsync(request, currentUserId, isAdmin);
        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateAttractionRequest request)
    {
        var userId = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : Guid.Empty;
        bool isAdmin = true;
        var id = await _attractionService.CreateAsync(request, userId, isAdmin);
        return CreatedAtAction(nameof(Search), new { id }, id);
    }

    [HttpPost("complete")]
    [AllowAnonymous]
    public async Task<ActionResult<Guid>> CreateComplete([FromBody] CreateCompleteAttractionRequest request)
    {
        var userId = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : Guid.Empty;
        bool isAdmin = true;
        var id = await _attractionService.CreateCompleteAsync(request, userId, isAdmin);
        return Ok(new { id, message = "Atracción completa creada con éxito." });
    }

    [HttpGet("top")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<AttractionSummaryResponse>>> GetTopRated([FromQuery] int count = 5)
    {
        var result = await _attractionService.GetTopRatedAsync(count);
        return Ok(result);
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<AttractionDetailResponse>> GetBySlug(string slug, [FromQuery] short requestedLangId = 1)
    {
        var result = await _attractionService.GetDetailBySlugAsync(slug, requestedLangId);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateAttractionRequest request)
    {
        var userId = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : Guid.Empty;
        bool isAdmin = true;
        var success = await _attractionService.UpdateAsync(id, request, userId, isAdmin);
        if (!success) return NotFound();
        return Ok(new { message = "Atracción actualizada con éxito." });
    }

    [HttpDelete("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult> Delete(Guid id)
    {
        var userId = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : Guid.Empty;
        var success = await _attractionService.DeleteAsync(id, userId, isAdmin: true);
        if (!success) return NotFound();
        return Ok(new { message = "Atracción eliminada con éxito." });
    }

    [HttpPatch("{id:guid}/status")]
    [AllowAnonymous]
    public async Task<ActionResult> ToggleStatus(Guid id, [FromBody] ToggleAttractionStatusRequest request)
    {
        var userId = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : Guid.Empty;
        bool isAdmin = true;
        var success = await _attractionService.ToggleStatusAsync(id, request.IsPublished, userId, isAdmin);
        if (!success) return NotFound();
        var estado = request.IsPublished ? "publicada" : "despublicada";
        return Ok(new { message = $"Atracción {estado} con éxito." });
    }

    [HttpPatch("{id:guid}/active")]
    [AllowAnonymous]
    public async Task<ActionResult> ToggleActive(Guid id, [FromBody] Microservicios.Atracciones.Catalog.Business.DTOs.Attraction.ToggleAttractionActiveRequest request)
    {
        var userId = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : Guid.Empty;
        bool isAdmin = true;
        
        try
        {
            var success = await _attractionService.ToggleActiveAsync(id, request.IsActive, userId, isAdmin);
            if (!success) return NotFound();
            
            var estado = request.IsActive ? "activada" : "desactivada";
            return Ok(new { message = $"Atracción {estado} con éxito." });
        }
        catch (Business.Exceptions.ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/complete")]
    [AllowAnonymous]
    public async Task<ActionResult<Microservicios.Atracciones.Catalog.Business.DTOs.Attraction.AttractionFullEditionResponse>> GetCompleteDetail(Guid id)
    {
        var userId = Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : Guid.Empty;
        bool isAdmin = true;

        var result = await _attractionService.GetCompleteByIdAsync(id, userId, isAdmin);
        if (result == null) return NotFound(new { message = "La atracción no existe o fue eliminada." });

        return Ok(result);
    }
}

