using MediatR;
using Vendix.Application.Common.Exceptions;
using Vendix.Application.Common.Interfaces;
using Vendix.Application.Common.Models;
using Vendix.Domain.Catalog.Repositories;
using Vendix.Domain.Catalog.ValueObjects;

namespace Vendix.Application.Catalog.Commands;

public record UpdateCategoryCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public Guid? ParentId { get; init; }
    public List<CategoryTranslationInput> Translations { get; init; } = [];
}

public class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateCategoryCommand, Result>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
        {
            throw new NotFoundException("Category", request.Id);
        }

        // Generate slug if not provided
        var slugValue = string.IsNullOrWhiteSpace(request.Slug) 
            ? Slug.FromText(request.Name) 
            : new Slug(request.Slug);

        // Check for duplicate slug (excluding current category)
        var existingBySlug = await categoryRepository.GetBySlugAsync(slugValue.Value, cancellationToken);
        if (existingBySlug is not null && existingBySlug.Id != request.Id)
        {
            throw new ConflictException("Category", "Slug", slugValue.Value);
        }

        // Validate parent (prevent self-reference and circular reference)
        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == request.Id)
            {
                throw new BusinessRuleException("SelfReference", "Category cannot be its own parent");
            }

            var parent = await categoryRepository.GetByIdAsync(request.ParentId.Value, cancellationToken);
            if (parent is null)
            {
                throw new NotFoundException("Category", request.ParentId.Value);
            }

            // TODO: Check for circular reference in hierarchy
        }

        // Update category
        category.UpdateName(request.Name);
        category.UpdateSlug(slugValue);
        category.SetParentCategory(request.ParentId);

        // Update translations - remove existing and add new ones
        var existingLanguageCodes = category.Translations.Select(t => t.LanguageCode).ToList();
        foreach (var langCode in existingLanguageCodes)
        {
            category.RemoveTranslation(langCode);
        }

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

        categoryRepository.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

