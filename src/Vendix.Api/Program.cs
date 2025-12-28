using System.Threading.RateLimiting;
using Vendix.Api.Middleware;
using Vendix.Application;
using Vendix.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add Application and Infrastructure services
builder.Services.AddApplication();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddInfrastructure(connectionString);

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString);

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Global rate limiter
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        // Partition by user identity or IP address
        var partitionKey = context.User.Identity?.Name
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: partitionKey,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 10
            });
    });

    // Specific policy for authentication endpoints
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.AutoReplenishment = true;
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    // Specific policy for search/heavy endpoints
    options.AddFixedWindowLimiter("search", limiterOptions =>
    {
        limiterOptions.AutoReplenishment = true;
        limiterOptions.PermitLimit = 30;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 5;
    });

    // What to do when rate limited
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Custom response when rate limited
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterTime)
            ? retryAfterTime.TotalSeconds
            : 60;

        context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            type = "TooManyRequests",
            title = "Too Many Requests",
            status = 429,
            detail = $"Rate limit exceeded. Please try again in {retryAfter} seconds.",
            retryAfterSeconds = retryAfter
        }, cancellationToken);
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Register exception handling middleware first
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Use Rate Limiting middleware
app.UseRateLimiter();

app.UseAuthorization();
app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

app.Run();
