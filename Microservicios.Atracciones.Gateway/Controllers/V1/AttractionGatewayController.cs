using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microservicios.Atracciones.Gateway.Models;
using Microsoft.Extensions.Configuration;

namespace Microservicios.Atracciones.Gateway.Controllers.V1
{
    [ApiController]
    [Route("api/v1/corrales-jorge/attraction")]
    public class AttractionGatewayController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _catalogBaseUrl;
        private readonly string _bookingBaseUrl;

        public AttractionGatewayController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            
            // Read addresses from reverse proxy config or default to the Azure environment URLs
            _catalogBaseUrl = configuration["ReverseProxy:Clusters:catalog-cluster:Destinations:destination1:Address"] 
                              ?? "https://ca-catalog.icycliff-cd7fde0b.eastus2.azurecontainerapps.io";
            _bookingBaseUrl = configuration["ReverseProxy:Clusters:booking-cluster:Destinations:destination1:Address"] 
                              ?? "https://ca-booking.icycliff-cd7fde0b.eastus2.azurecontainerapps.io";

            _catalogBaseUrl = _catalogBaseUrl.TrimEnd('/');
            _bookingBaseUrl = _bookingBaseUrl.TrimEnd('/');
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseListAtraccion>> ListarAtracciones(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] bool? disponible = null)
        {
            try
            {
                // Catalog search query
                var queryParams = $"?PageNumber={page}&PageSize={pageSize}";
                if (!string.IsNullOrEmpty(search))
                {
                    queryParams += $"&SearchTerm={Uri.EscapeDataString(search)}";
                }

                var catalogResponse = await _httpClient.GetAsync($"{_catalogBaseUrl}/api/v1/corrales-jorge/attraction{queryParams}");
                if (!catalogResponse.IsSuccessStatusCode)
                {
                    return StatusCode((int)catalogResponse.StatusCode, new ApiResponseListAtraccion
                    {
                        Success = false,
                        Message = "Error al consultar el catálogo de atracciones.",
                        Errors = new List<string> { $"Código de estado backend: {catalogResponse.StatusCode}" }
                    });
                }

                var catalogContent = await catalogResponse.Content.ReadAsStringAsync();
                var catalogResult = JsonSerializer.Deserialize<CatalogPagedResult<CatalogAttractionSummary>>(catalogContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (catalogResult == null || catalogResult.Items == null)
                {
                    return Ok(new ApiResponseListAtraccion { Success = true, Message = "No se encontraron atracciones." });
                }

                // Check live availability for all attractions in parallel
                var tasks = catalogResult.Items.Select(async item =>
                {
                    var hasAvailability = await CheckAvailabilityAsync(item.Id);
                    return new AtraccionDto
                    {
                        Id = item.Id,
                        Nombre = item.Name,
                        Descripcion = item.DescriptionShort,
                        Precio = item.StartingPrice ?? 0,
                        Moneda = item.CurrencyCode ?? "USD",
                        Ubicacion = item.LocationName,
                        ImagenUrl = item.MainImageUrl,
                        Disponible = hasAvailability,
                        Slug = item.Slug
                    };
                }).ToList();

                var mappedList = await Task.WhenAll(tasks);
                var finalData = mappedList.ToList();

                // If filter 'disponible' is active, filter list
                if (disponible.HasValue && disponible.Value)
                {
                    finalData = finalData.Where(a => a.Disponible).ToList();
                }

                return Ok(new ApiResponseListAtraccion
                {
                    Success = true,
                    Data = finalData
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseListAtraccion
                {
                    Success = false,
                    Message = "Ocurrió un error interno en el Gateway API.",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("{slug}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseAtraccionDetalle>> DetalleAtraccion(string slug)
        {
            try
            {
                // 1. Fetch static details from Catalog
                var catalogResponse = await _httpClient.GetAsync($"{_catalogBaseUrl}/api/v1/corrales-jorge/attraction/{slug}");
                if (catalogResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound(new ApiResponseAtraccionDetalle
                    {
                        Success = false,
                        Message = "Atracción no encontrada."
                    });
                }
                
                if (!catalogResponse.IsSuccessStatusCode)
                {
                    return StatusCode((int)catalogResponse.StatusCode, new ApiResponseAtraccionDetalle
                    {
                        Success = false,
                        Message = "Error al obtener detalles de la atracción del catálogo."
                    });
                }

                var catalogContent = await catalogResponse.Content.ReadAsStringAsync();
                var attractionDetail = JsonSerializer.Deserialize<CatalogAttractionDetail>(catalogContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (attractionDetail == null)
                {
                    return NotFound(new ApiResponseAtraccionDetalle
                    {
                        Success = false,
                        Message = "Atracción no encontrada al deserializar catálogo."
                    });
                }

                // 2. Fetch live availability slots from Booking
                var slotsList = await GetAvailabilitySlotsAsync(attractionDetail.Id);
                var hasAvailability = slotsList.Any(d => d.CuposDisponibles > 0 && d.Horarios.Any(h => h.CuposDisponibles > 0));

                // 3. Compute price details
                var minPrice = attractionDetail.Products.SelectMany(p => p.PriceTiers).Select(pt => pt.Price).DefaultIfEmpty(0).Min();
                var currency = attractionDetail.Products.SelectMany(p => p.PriceTiers).Select(pt => pt.CurrencyCode).FirstOrDefault() ?? "USD";
                var mainImage = attractionDetail.Gallery.FirstOrDefault(m => m.IsMain)?.Url ?? attractionDetail.Gallery.FirstOrDefault()?.Url;

                // 4. Map modalities & slots
                var modalities = attractionDetail.Products.Select(prod => new ModalidadDto
                {
                    Id = prod.Id,
                    Nombre = prod.Title,
                    Descripcion = prod.Description,
                    Slots = slotsList.SelectMany(day => day.Horarios.Select(h => new SlotDto
                    {
                        Id = h.SlotId,
                        Fecha = day.Fecha,
                        HoraInicio = h.HoraInicio,
                        CuposDisponibles = h.CuposDisponibles,
                        CapacidadTotal = h.CuposTotales
                    })).ToList()
                }).ToList();

                var detailDto = new AtraccionDetalleDto
                {
                    Id = attractionDetail.Id,
                    Nombre = attractionDetail.Name,
                    Descripcion = attractionDetail.DescriptionFull ?? attractionDetail.DescriptionShort,
                    Precio = minPrice,
                    Moneda = currency,
                    Ubicacion = attractionDetail.LocationName,
                    ImagenUrl = mainImage,
                    Disponible = hasAvailability,
                    Slug = attractionDetail.Slug,
                    Modalidades = modalities
                };

                return Ok(new ApiResponseAtraccionDetalle
                {
                    Success = true,
                    Data = detailDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseAtraccionDetalle
                {
                    Success = false,
                    Message = $"Ocurrió un error en el Gateway: {ex.Message}"
                });
            }
        }

        private async Task<bool> CheckAvailabilityAsync(Guid attractionId)
        {
            try
            {
                var slots = await GetAvailabilitySlotsAsync(attractionId);
                return slots.Any(day => day.CuposDisponibles > 0 && day.Horarios.Any(h => h.CuposDisponibles > 0));
            }
            catch
            {
                return false;
            }
        }

        private async Task<List<BookingAvailabilityDay>> GetAvailabilitySlotsAsync(Guid attractionId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_bookingBaseUrl}/api/v1/corrales-jorge/booking/{attractionId}/availability");
                if (!response.IsSuccessStatusCode) return new List<BookingAvailabilityDay>();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<BookingApiResponse<List<BookingAvailabilityDay>>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Data ?? new List<BookingAvailabilityDay>();
            }
            catch
            {
                return new List<BookingAvailabilityDay>();
            }
        }

        // Helper classes for Deserializing backend responses
        private class CatalogPagedResult<T>
        {
            public List<T> Items { get; set; } = new();
            public int TotalCount { get; set; }
        }

        private class CatalogAttractionSummary
        {
            public Guid Id { get; set; }
            public string Slug { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string? DescriptionShort { get; set; }
            public string LocationName { get; set; } = string.Empty;
            public string? MainImageUrl { get; set; }
            public decimal? StartingPrice { get; set; }
            public string CurrencyCode { get; set; } = "USD";
        }

        private class CatalogAttractionDetail
        {
            public Guid Id { get; set; }
            public string Slug { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string? DescriptionShort { get; set; }
            public string? DescriptionFull { get; set; }
            public string LocationName { get; set; } = string.Empty;
            public List<CatalogMedia> Gallery { get; set; } = new();
            public List<CatalogProduct> Products { get; set; } = new();
        }

        private class CatalogMedia
        {
            public string Url { get; set; } = string.Empty;
            public bool IsMain { get; set; }
        }

        private class CatalogProduct
        {
            public Guid Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public List<CatalogPriceTier> PriceTiers { get; set; } = new();
        }

        private class CatalogPriceTier
        {
            public decimal Price { get; set; }
            public string CurrencyCode { get; set; } = "USD";
        }

        private class BookingApiResponse<T>
        {
            public bool Success { get; set; }
            public T? Data { get; set; }
        }

        private class BookingAvailabilityDay
        {
            public string Fecha { get; set; } = string.Empty;
            public int CuposDisponibles { get; set; }
            public List<BookingHorario> Horarios { get; set; } = new();
        }

        private class BookingHorario
        {
            public Guid SlotId { get; set; }
            public string HoraInicio { get; set; } = string.Empty;
            public int CuposDisponibles { get; set; }
            public int CuposTotales { get; set; }
        }
    }
}
