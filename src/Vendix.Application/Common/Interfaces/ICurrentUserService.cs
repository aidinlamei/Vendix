namespace Vendix.Application.Common.Interfaces;

/// <summary>
/// Provides information about the currently authenticated user.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the current user, or null if not authenticated.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the username of the current user, or null if not authenticated.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Determines whether the current user is in the specified role.
    /// </summary>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the user is in the role; otherwise, false.</returns>
    bool IsInRole(string role);
}
