using MediatR;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Catalog.Entities;
using Vendix.Domain.Catalog.Repositories;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Application.Catalog.Commands;

public record CreateCategoryCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public Guid? ParentId { get; init; }
    public List<CategoryTranslationInput> Translations { get; init; } = [];
}

public record CategoryTranslationInput
{
    public string LanguageCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Generate slug if not provided
        var slugValue = string.IsNullOrWhiteSpace(request.Slug) 
            ? Slug.FromText(request.Name) 
            : new Slug(request.Slug);

        // Check for duplicate slug
        var existingBySlug = await categoryRepository.GetBySlugAsync(slugValue.Value, cancellationToken);
        if (existingBySlug is not null)
        {
            throw new ConflictException("Category", "Slug", slugValue.Value);
        }

        // Validate parent exists if provided
        Category? parent = null;
        if (request.ParentId.HasValue)
        {
            parent = await categoryRepository.GetByIdAsync(request.ParentId.Value, cancellationToken);
            if (parent is null)
            {
                throw new NotFoundException("Category", request.ParentId.Value);
            }
        }

        // Create category (Category entity doesn't have Description property)
        var category = new Category(request.Name, slugValue, request.ParentId);

        // Add translations (including description in translation if provided)
        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            // Add default English translation with description if no translations provided
            if (request.Translations.Count == 0)
            {
                category.AddTranslation("en", request.Name, request.Description);
            }
            else
            {
                // Add description to first translation if it doesn't have one
                var firstTranslation = request.Translations[0];
                category.AddTranslation(
                    firstTranslation.LanguageCode,
                    firstTranslation.Name,
                    firstTranslation.Description ?? request.Description);
                
                // Add remaining translations
                for (int i = 1; i < request.Translations.Count; i++)
                {
                    var translation = request.Translations[i];
                    category.AddTranslation(translation.LanguageCode, translation.Name, translation.Description);
                }
            }
        }
        else
        {
            // Add all translations
            foreach (var translation in request.Translations)
            {
                category.AddTranslation(translation.LanguageCode, translation.Name, translation.Description);
            }
        }

        await categoryRepository.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await cacheService.RemoveByPrefixAsync("categories", cancellationToken);

        return Result<Guid>.Success(category.Id);
    }
}

