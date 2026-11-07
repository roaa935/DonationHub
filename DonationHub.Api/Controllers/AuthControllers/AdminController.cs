
using DonationHub.Application.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonationHub.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAuthService _authService;

    public AdminController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("staff")]
    public async Task<IActionResult> CreateStaff(
        CreateStaffRequest request)
    {
        try
        {
            var result = await _authService.CreateStaffAsync(request);

            return Ok(result);
        }
        catch (AuthException ex)
        {
            return StatusCode(
                ex.StatusCode,
                new { code = ex.Code });
        }
    }
}
