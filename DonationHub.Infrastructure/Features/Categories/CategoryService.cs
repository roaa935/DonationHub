
using DonationHub.Application.Persistence;
using DonationHub.Domain.Entities;

namespace DonationHub.Application.Features.Categories;

public class CategoryService : ICategoryService
{
    private readonly IGenericRepository<Category> _categoryRepository;
    private readonly IGenericRepository<Campaign> _campaignRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(
        IGenericRepository<Category> categoryRepository,
        IGenericRepository<Campaign> campaignRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _campaignRepository = campaignRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request)
    {
        var nameAr = request.NameAr.Trim();
        var nameEn = request.NameEn.Trim();

        await EnsureNamesAreUnique(nameAr, nameEn);

        var category = Category.Create(
            nameAr,
            nameEn,
            request.DescriptionAr,
            request.DescriptionEn,
            DateTime.UtcNow);

        await _categoryRepository.AddAsync(category);

        await _unitOfWork.CompleteAsync();

        return MapToResponse(category);
    }

    public async Task<CategoryResponse> UpdateAsync(
        Guid id,
        UpdateCategoryRequest request)
    {
        var category = await GetCategoryOrThrow(id);

        var nameAr = request.NameAr.Trim();
        var nameEn = request.NameEn.Trim();

        await EnsureNamesAreUnique(nameAr, nameEn, id);

        category.Update(
            nameAr,
            nameEn,
            request.DescriptionAr,
            request.DescriptionEn,
            DateTime.UtcNow);

        await _unitOfWork.CompleteAsync();

        return MapToResponse(category);
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await GetCategoryOrThrow(id);

        var hasCampaigns = await _campaignRepository.AnyAsync(
            campaign => campaign.CategoryId == id);

        if (hasCampaigns)
        {
            throw new CategoryException(
                "Category.HasCampaigns", 409);
        }

        _categoryRepository.Remove(category);

        await _unitOfWork.CompleteAsync();
    }

    public async Task<CategoryResponse> GetByIdAsync(Guid id)
    {
        var category = await GetCategoryOrThrow(id);

        return MapToResponse(category);
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();

        return categories.Select(MapToResponse).ToList();
    }

    private async Task<Category> GetCategoryOrThrow(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category is null)
        {
            throw new CategoryException(
                "Category.NotFound", 404);
        }

        return category;
    }

    private async Task EnsureNamesAreUnique(
        string nameAr,
        string nameEn,
        Guid? excludedId = null)
    {
        var arabicExists = await _categoryRepository.AnyAsync(
            c => c.NameAr == nameAr &&
                 (excludedId == null || c.Id != excludedId.Value));

        if (arabicExists)
        {
            throw new CategoryException(
                "Category.NameArAlreadyExists", 409);
        }

        var englishExists = await _categoryRepository.AnyAsync(
            c => c.NameEn == nameEn &&
                 (excludedId == null || c.Id != excludedId.Value));

        if (englishExists)
        {
            throw new CategoryException(
                "Category.NameEnAlreadyExists", 409);
        }
    }

    private static CategoryResponse MapToResponse(Category category)
    {
        return new CategoryResponse(
            category.Id,
            category.NameAr,
            category.NameEn,
            category.DescriptionAr,
            category.DescriptionEn);
    }
}
