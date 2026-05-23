using System;
using System.Collections.Generic;

namespace Microservicios.Atracciones.Gateway.Models
{
    public class AtraccionDto
    {
        public Guid Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? DescriptionShort { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string LocationCountryCode { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? SubcategoryName { get; set; }
        public decimal RatingAverage { get; set; }
        public int RatingCount { get; set; }
        public string? DifficultyLevel { get; set; }
        public string? MainImageUrl { get; set; }
        public decimal StartingPrice { get; set; }
        public string CurrencyCode { get; set; } = "USD";
        public bool IsActive { get; set; }
        public bool IsPublished { get; set; }
        public int ModalityCount { get; set; }
        public bool Disponible { get; set; }

        // Backwards compatibility mappings for Spanish keys
        public string Nombre => Name;
        public string? Descripcion => DescriptionShort;
        public decimal Precio => StartingPrice;
        public string Moneda => CurrencyCode;
        public string? Ubicacion => LocationName;
        public string? ImagenUrl => MainImageUrl;
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
        public string Name => Nombre;
        public string? Description => Descripcion;
        public List<SlotDto> Slots { get; set; } = new();
    }

    public class SlotDto
    {
        public Guid Id { get; set; }
        public string Fecha { get; set; } = string.Empty;
        public string HoraInicio { get; set; } = string.Empty;
        public int CuposDisponibles { get; set; }
        public int CapacidadTotal { get; set; }
        public string StartTime => HoraInicio;
        public int CapacityAvailable => CuposDisponibles;
        public int CapacityTotal => CapacidadTotal;
    }

    public class ApiResponseListAtraccion
    {
        public bool Success { get; set; }
        public AttractionListData Data { get; set; } = new AttractionListData();
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class AttractionListData
    {
        public List<AtraccionDto> Items { get; set; } = new List<AtraccionDto>();
    }

    public class ApiResponseAtraccionDetalle
    {
        public bool Success { get; set; }
        public AtraccionDetalleDto? Data { get; set; }
        public string? Message { get; set; }
    }
}
