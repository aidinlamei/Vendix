using FluentValidation;

namespace Vendix.Application.Catalog.Commands;

public class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Brand ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

        RuleFor(x => x.Slug)
            .MaximumLength(100).WithMessage("Slug must not exceed 100 characters")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .When(x => !string.IsNullOrEmpty(x.Slug))
            .WithMessage("Slug must be lowercase alphanumeric with hyphens");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("Logo URL must not exceed 500 characters")
            .Must(BeAValidUrl)
            .When(x => !string.IsNullOrEmpty(x.LogoUrl))
            .WithMessage("Logo URL must be a valid URL");
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        
        // Accept relative URLs (starting with /)
        if (url.StartsWith("/"))
            return true;
        
        // Accept absolute URLs (http/https)
        return Uri.TryCreate(url, UriKind.Absolute, out var result) 
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

