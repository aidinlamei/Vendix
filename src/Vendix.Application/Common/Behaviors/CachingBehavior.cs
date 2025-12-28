using System.Reflection;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Vendix.Application.Common.Attributes;
using Vendix.Application.Common.Interfaces;

namespace Vendix.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that caches query results.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
/// <remarks>
/// This behavior only applies to requests decorated with <see cref="CacheableQueryAttribute"/>.
/// It checks the cache before executing the query and stores the result after execution.
/// </remarks>
public sealed class CachingBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<CachingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Handles the caching pipeline.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cached or newly computed response.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Check if request has CacheableQuery attribute
        var cacheAttribute = typeof(TRequest).GetCustomAttribute<CacheableQueryAttribute>();
        if (cacheAttribute is null)
        {
            return await next();
        }

        // Check if cache bypass is requested
        if (cacheAttribute.BypassCache)
        {
            logger.LogDebug("Cache bypass requested for {RequestName}", typeof(TRequest).Name);
            return await next();
        }

        // Generate cache key
        var cacheKey = GenerateCacheKey(request, cacheAttribute);

        // Try to get from cache
        var cachedResponse = await cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResponse is not null)
        {
            logger.LogDebug("Cache hit for {RequestName} with key {CacheKey}", typeof(TRequest).Name, cacheKey);
            return cachedResponse;
        }

        // Execute the handler
        logger.LogDebug("Cache miss for {RequestName} with key {CacheKey}", typeof(TRequest).Name, cacheKey);
        var response = await next();

        // Cache the result
        if (response is not null)
        {
            var expiry = TimeSpan.FromMinutes(cacheAttribute.ExpiryMinutes);
            await cacheService.SetAsync(cacheKey, response, expiry, cancellationToken);
            logger.LogDebug(
                "Cached response for {RequestName} with key {CacheKey} for {ExpiryMinutes} minutes",
                typeof(TRequest).Name,
                cacheKey,
                cacheAttribute.ExpiryMinutes);
        }

        return response;
    }

    /// <summary>
    /// Generates a cache key for the request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="attribute">The cache attribute.</param>
    /// <returns>The generated cache key.</returns>
    private static string GenerateCacheKey(TRequest request, CacheableQueryAttribute attribute)
    {
        // If a custom key is specified, use it
        if (!string.IsNullOrWhiteSpace(attribute.Key))
        {
            return $"{attribute.Key}:{SerializeRequest(request)}";
        }

        // Auto-generate key from type name and request properties
        var typeName = typeof(TRequest).Name.Replace("Query", "").ToLowerInvariant();
        var requestHash = SerializeRequest(request);

        return $"query:{typeName}:{requestHash}";
    }

    /// <summary>
    /// Serializes the request to a hash string for cache key generation.
    /// </summary>
    /// <param name="request">The request to serialize.</param>
    /// <returns>A hash representation of the request.</returns>
    private static string SerializeRequest(TRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            // Create a simple hash from the JSON
            var hash = json.GetHashCode(StringComparison.Ordinal);
            return hash.ToString("x8");
        }
        catch
        {
            // Fallback to type name hash if serialization fails
            return typeof(TRequest).FullName?.GetHashCode().ToString("x8") ?? "unknown";
        }
    }
}
