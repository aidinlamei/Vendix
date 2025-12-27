using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Vendix.Application.Common.Interfaces;

namespace Vendix.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs requests and their execution times.
/// </summary>
/// <typeparam name="TRequest">The type of request being logged.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentUserService? currentUserService = null) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int SlowRequestThresholdMs = 500;

    /// <summary>
    /// Handles the logging pipeline.
    /// </summary>
    /// <param name="request">The request to log.</param>
    /// <param name="next">The next handler in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response from the next handler.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = currentUserService?.UserId ?? "Anonymous";
        var userName = currentUserService?.UserName ?? "Anonymous";
        var timestamp = DateTime.UtcNow;

        logger.LogInformation(
            "Handling {RequestName} for user {UserId} ({UserName}) at {Timestamp}",
            requestName,
            userId,
            userName,
            timestamp);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (elapsedMs > SlowRequestThresholdMs)
            {
                logger.LogWarning(
                    "Slow request: {RequestName} took {ElapsedMilliseconds}ms for user {UserId}",
                    requestName,
                    elapsedMs,
                    userId);
            }
            else
            {
                logger.LogInformation(
                    "Handled {RequestName} in {ElapsedMilliseconds}ms",
                    requestName,
                    elapsedMs);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "Error handling {RequestName} for user {UserId} after {ElapsedMilliseconds}ms",
                requestName,
                userId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
