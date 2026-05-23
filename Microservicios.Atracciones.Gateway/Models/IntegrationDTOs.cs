using System;
using System.Collections.Generic;

namespace Microservicios.Atracciones.Gateway.Models
{
    public class AtraccionDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string Moneda { get; set; } = "USD";
        public string? Ubicacion { get; set; }
        public string? ImagenUrl { get; set; }
        public bool Disponible { get; set; }
        public string Slug { get; set; } = string.Empty;
    }

    public class AtraccionDetalleDto : AtraccionDto
    {
        public List<ModalidadDto> Modalidades { get; set; } = new();
    }

    public class ModalidadDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public List<SlotDto> Slots { get; set; } = new();
    }

    public class SlotDto
    {
        public Guid Id { get; set; }
        public string Fecha { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty;
        public int CuposDisponibles { get; set; }
        public int CapacidadTotal { get; set; }
    }

    public class ApiResponseListAtraccion
    {
        public bool Success { get; set; }
        public List<AtraccionDto> Data { get; set; } = new();
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ApiResponseAtraccionDetalle
    {
        public bool Success { get; set; }
        public AtraccionDetalleDto? Data { get; set; }
        public string? Message { get; set; }
    }
}
