
namespace DonationHub.Application.Features.Categories;

public interface ICategoryService
{
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);

    Task<CategoryResponse> UpdateAsync(
        Guid id,
        UpdateCategoryRequest request);

    Task DeleteAsync(Guid id);

    Task<CategoryResponse> GetByIdAsync(Guid id);

    Task<IReadOnlyList<CategoryResponse>> GetAllAsync();
}
