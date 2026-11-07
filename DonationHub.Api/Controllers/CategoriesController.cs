
using DonationHub.Application.Features.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonationHub.Api.Controllers;

[ApiController]
[Route("api/v1/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _categoryService.GetAllAsync());
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            return Ok(await _categoryService.GetByIdAsync(id));
        }
        catch (CategoryException ex)
        {
            return StatusCode(ex.StatusCode, new { code = ex.Code });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequest request)
    {
        try
        {
            var result = await _categoryService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (CategoryException ex)
        {
            return StatusCode(ex.StatusCode, new { code = ex.Code });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateCategoryRequest request)
    {
        try
        {
            return Ok(await _categoryService.UpdateAsync(id, request));
        }
        catch (CategoryException ex)
        {
            return StatusCode(ex.StatusCode, new { code = ex.Code });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _categoryService.DeleteAsync(id);

            return NoContent();
        }
        catch (CategoryException ex)
        {
            return StatusCode(ex.StatusCode, new { code = ex.Code });
        }
    }
}
