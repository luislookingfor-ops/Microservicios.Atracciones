using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microservicios.Atracciones.Booking.API.Filters
{
    public class IdempotencyFilter : IAsyncActionFilter
    {
        private readonly IMemoryCache _cache;
        private const string IdempotencyHeader = "X-Idempotency-Key";

        public IdempotencyFilter(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Solo aplicar a solicitudes POST (creación de reservas/cancelaciones)
            if (context.HttpContext.Request.Method != "POST")
            {
                await next();
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue(IdempotencyHeader, out var headerValue) || string.IsNullOrEmpty(headerValue))
            {
                await next();
                return;
            }

            var cacheKey = $"idempotency:{headerValue}";

            if (_cache.TryGetValue(cacheKey, out string? cachedValue))
            {
                if (cachedValue == "processing")
                {
                    context.Result = new ConflictObjectResult(new { message = "La petición está siendo procesada, por favor espere." });
                    return;
                }

                if (cachedValue != null)
                {
                    var response = JsonSerializer.Deserialize<CachedResponse>(cachedValue);
                    if (response != null)
                    {
                        context.Result = new ContentResult
                        {
                            Content = response.Body,
                            ContentType = response.ContentType,
                            StatusCode = response.StatusCode
                        };
                        return;
                    }
                }
            }

            // Registrar en caché que está procesando
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            };
            _cache.Set(cacheKey, "processing", cacheOptions);

            var executedContext = await next();

            if (executedContext.Exception != null && !executedContext.ExceptionHandled)
            {
                // Si hubo un error, remover de la caché para permitir reintento
                _cache.Remove(cacheKey);
                return;
            }

            if (executedContext.Result is ObjectResult objectResult && objectResult.StatusCode >= 200 && objectResult.StatusCode < 300)
            {
                var body = JsonSerializer.Serialize(objectResult.Value);
                var responseToCache = new CachedResponse
                {
                    StatusCode = objectResult.StatusCode ?? 200,
                    ContentType = "application/json",
                    Body = body
                };

                var cacheOptionsSuccess = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                };
                _cache.Set(cacheKey, JsonSerializer.Serialize(responseToCache), cacheOptionsSuccess);
            }
            else
            {
                // Si no fue exitoso (2xx), remover de la caché
                _cache.Remove(cacheKey);
            }
        }

        private class CachedResponse
        {
            public int StatusCode { get; set; }
            public string ContentType { get; set; } = "application/json";
            public string Body { get; set; } = string.Empty;
        }
    }
}
