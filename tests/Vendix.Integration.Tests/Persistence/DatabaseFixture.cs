using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Vendix.Infrastructure.Persistence;

namespace Vendix.Integration.Tests.Persistence;

/// <summary>
/// Spins up a disposable PostgreSQL 16 container for the lifetime of the test collection and
/// exposes freshly-configured <see cref="VendixDbContext"/> instances against it. Migrations are
/// applied once during <see cref="InitializeAsync"/>. Each test should call <see cref="CreateContext"/>
/// per arrange/act/assert step rather than reusing one context, to avoid EF Core's change tracker
/// masking real persistence bugs.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("vendix_test")
        .WithUsername("vendix")
        .WithPassword("vendix")
        .Build();

    /// <summary>
    /// Starts the PostgreSQL container and applies all EF Core migrations against it.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Stops and removes the PostgreSQL container.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    /// <summary>
    /// Creates a new <see cref="VendixDbContext"/> pointed at the running container.
    /// </summary>
    public VendixDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<VendixDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        return new VendixDbContext(options);
    }
}

/// <summary>
/// xUnit collection definition so all fixture-consuming test classes share one container
/// instead of each spinning up its own (which would be correct but slow).
/// </summary>
[CollectionDefinition(nameof(DatabaseCollection))]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
}

[Collection(nameof(DatabaseCollection))]
public class DatabaseFixtureTests
{
    private readonly DatabaseFixture _fixture;

    public DatabaseFixtureTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateContext_AfterInitialize_CanConnectAndHasAppliedMigrations()
    {
        await using var context = _fixture.CreateContext();

        var canConnect = await context.Database.CanConnectAsync();
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

        canConnect.Should().BeTrue();
        pendingMigrations.Should().BeEmpty();
    }
}
