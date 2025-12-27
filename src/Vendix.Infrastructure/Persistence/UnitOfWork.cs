using Vendix.Application.Common.Interfaces;

namespace Vendix.Infrastructure.Persistence;

/// <summary>
/// Implementation of the unit of work pattern using Entity Framework Core.
/// </summary>
/// <remarks>
/// This class wraps the DbContext's SaveChangesAsync method to provide
/// a clean abstraction for persisting changes across multiple repositories.
/// </remarks>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly VendixDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UnitOfWork(VendixDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
