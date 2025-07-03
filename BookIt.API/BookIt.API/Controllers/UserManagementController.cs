using BookIt.API.Models.Requests;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookIt.API.Controllers;

[ApiController]
[Route("user")]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UserManagementController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpPost("images")]
    [Authorize]
    public async Task<IActionResult> SetUserImages([FromBody] UserImagesRequest request)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int userId = int.Parse(userIdStr!);

        var imagesDto = request
            .ExistingPhotosIds
            .Select(id => new ImageDTO { Id = id })
            .Union(request.NewPhotosBase64
                .Select(base64 => new ImageDTO { Base64Image = base64 }));

        await _userManagementService.SetUserImagesAsync(userId, imagesDto);

        return Ok(new { Message = "Images updated successfully." });
    }
}
