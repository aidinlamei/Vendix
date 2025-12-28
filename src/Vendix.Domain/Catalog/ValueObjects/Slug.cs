using System.Text;
using System.Text.RegularExpressions;
using Vendix.Domain.Common;

namespace Vendix.Domain.Catalog.ValueObjects;

/// <summary>
/// Represents a URL-friendly slug for SEO and routing.
/// </summary>
/// <remarks>
/// Slugs are used in URLs to identify resources in a human-readable format.
/// They must be lowercase, contain only letters, numbers, and hyphens,
/// and cannot start or end with a hyphen.
/// </remarks>
public sealed partial class Slug : ValueObject
{
    /// <summary>
    /// The minimum length allowed for a slug.
    /// </summary>
    public const int MinLength = 2;

    /// <summary>
    /// The maximum length allowed for a slug.
    /// </summary>
    public const int MaxLength = 200;

    /// <summary>
    /// The regex pattern for validating slug format: lowercase letters, numbers, and hyphens only.
    /// </summary>
    public const string Pattern = @"^[a-z0-9\-]+$";

    /// <summary>
    /// Gets the slug value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Slug"/> class.
    /// </summary>
    /// <param name="value">The slug value. Must be a valid URL-friendly format.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the value is null, empty, or not in a valid slug format.
    /// </exception>
    public Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Slug is required.", nameof(value));
        }

        var normalized = value.ToLowerInvariant().Trim();

        if (normalized.Length < MinLength)
        {
            throw new ArgumentException($"Slug must be at least {MinLength} characters.", nameof(value));
        }

        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException($"Slug cannot exceed {MaxLength} characters.", nameof(value));
        }

        if (!SlugPattern().IsMatch(normalized))
        {
            throw new ArgumentException("Slug must contain only lowercase letters, numbers, and hyphens.", nameof(value));
        }

        if (normalized.StartsWith('-') || normalized.EndsWith('-'))
        {
            throw new ArgumentException("Slug cannot start or end with a hyphen.", nameof(value));
        }

        if (normalized.Contains("--"))
        {
            throw new ArgumentException("Slug cannot contain consecutive hyphens.", nameof(value));
        }

        Value = normalized;
    }

    /// <summary>
    /// Creates a slug from a given text by converting it to a URL-friendly format.
    /// </summary>
    /// <param name="text">The text to convert to a slug.</param>
    /// <returns>A new Slug instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the text cannot be converted to a valid slug.</exception>
    public static Slug FromText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text is required.", nameof(text));
        }

        var slug = text.ToLowerInvariant().Trim();

        // Replace spaces and underscores with hyphens
        slug = slug.Replace(' ', '-').Replace('_', '-');

        // Remove invalid characters
        slug = InvalidCharPattern().Replace(slug, "");

        // Replace multiple hyphens with single hyphen
        slug = MultipleHyphenPattern().Replace(slug, "-");

        // Remove leading and trailing hyphens
        slug = slug.Trim('-');

        // Ensure minimum length
        if (slug.Length < MinLength)
        {
            throw new ArgumentException($"The resulting slug is too short. Minimum length is {MinLength} characters.", nameof(text));
        }

        // Truncate if too long
        if (slug.Length > MaxLength)
        {
            slug = slug[..MaxLength].TrimEnd('-');
        }

        return new Slug(slug);
    }

    /// <summary>
    /// Validates whether the given string is a valid slug format.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>True if the value is a valid slug format; otherwise, false.</returns>
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.ToLowerInvariant().Trim();

        if (normalized.Length < MinLength || normalized.Length > MaxLength)
        {
            return false;
        }

        if (!SlugPattern().IsMatch(normalized))
        {
            return false;
        }

        if (normalized.StartsWith('-') || normalized.EndsWith('-'))
        {
            return false;
        }

        if (normalized.Contains("--"))
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Returns the slug value as a string.
    /// </summary>
    /// <returns>The slug value.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a Slug to its string value.
    /// </summary>
    /// <param name="slug">The Slug to convert.</param>
    public static implicit operator string(Slug slug) => slug.Value;

    /// <summary>
    /// Regex pattern for validating slug format: lowercase letters, numbers, and hyphens only.
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9\-]+$", RegexOptions.Compiled)]
    private static partial Regex SlugPattern();

    /// <summary>
    /// Regex pattern for matching invalid characters in slug generation.
    /// </summary>
    [GeneratedRegex(@"[^a-z0-9\-]", RegexOptions.Compiled)]
    private static partial Regex InvalidCharPattern();

    /// <summary>
    /// Regex pattern for matching multiple consecutive hyphens.
    /// </summary>
    [GeneratedRegex(@"-+", RegexOptions.Compiled)]
    private static partial Regex MultipleHyphenPattern();
}
