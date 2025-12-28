using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Vendix.Application.Common.Attributes;
using Vendix.Application.Common.Interfaces;

namespace Vendix.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that caches query results for queries marked with CacheableQueryAttribute.
/// </summary>
/// <typeparam name="TRequest">The type of request being processed.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
/// <remarks>
/// This behavior only applies to queries (read operations).
/// Commands (write operations) bypass caching.
/// </remarks>
public sealed class CachingBehavior<TRequest, TResponse>(
    ICacheService cacheService,
    ILogger<CachingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Handles the caching pipeline.
    /// </summary>
    /// <param name="request">The request to process.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cached response or the response from the next handler.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Check if request has CacheableQuery attribute
        var cacheAttribute = typeof(TRequest).GetCustomAttributes(typeof(CacheableQueryAttribute), false)
            .FirstOrDefault() as CacheableQueryAttribute;

        if (cacheAttribute is null)
        {
            // No caching for this request
            return await next();
        }

        // Generate cache key based on attribute key and request parameters
        var cacheKey = GenerateCacheKey(cacheAttribute.Key, request);

        // Try to get from cache
        var cachedResponse = await cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);

        if (cachedResponse is not null)
        {
            logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return cachedResponse;
        }

        logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);

        // Execute handler
        var response = await next();

        // Cache the response
        var expiry = TimeSpan.FromMinutes(cacheAttribute.ExpiryMinutes);
        await cacheService.SetAsync(cacheKey, response, expiry, cancellationToken);

        logger.LogDebug("Cached response for key: {CacheKey} with expiry: {Expiry} minutes",
            cacheKey, cacheAttribute.ExpiryMinutes);

        return response;
    }

    /// <summary>
    /// Generates a cache key by combining the base key with a hash of the request parameters.
    /// </summary>
    /// <param name="baseKey">The base cache key from the attribute.</param>
    /// <param name="request">The request object.</param>
    /// <returns>A unique cache key for this request.</returns>
    private static string GenerateCacheKey(string baseKey, TRequest request)
    {
        // For simple requests, just use the base key
        // For parameterized requests, append a hash of the parameters
        var requestType = typeof(TRequest);
        var properties = requestType.GetProperties();

        if (properties.Length == 0)
        {
            return baseKey;
        }

        // Serialize request to JSON for a consistent hash
        var requestJson = JsonSerializer.Serialize(request);
        var requestHash = requestJson.GetHashCode();

        return $"{baseKey}:{requestHash}";
    }
}
