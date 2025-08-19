using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookIt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Tenant,Landlord,Admin")]
public class UserManagementController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IUserManagementService _userManagementService;

    public UserManagementController(
        IMapper mapper,
        IUserService userService,
        IUserManagementService userManagementService)
    {
        _mapper = mapper;
        _userService = userService;
        _userManagementService = userManagementService;
    }

    [HttpPut("images")]
    public async Task<IActionResult> SetUserImages([FromBody] UserImagesRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var imagesDto = request
            .ExistingPhotosIds
            .Select(id => new ImageDTO { Id = id })
            .Union(request.NewPhotosBase64
                .Select(base64 => new ImageDTO { Base64Image = base64 }));

        await _userManagementService.SetUserImagesAsync(userId, imagesDto);

        return Ok(new { Message = "Images updated successfully." });
    }

    [HttpGet("images")]
    public async Task<IActionResult> GetUserImages()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var imageDtos = await _userManagementService.GetUserImagesAsync(userId);
        var imagesResponse = _mapper.Map<IEnumerable<ImageResponse>>(imageDtos);

        return Ok(imagesResponse);
    }

    [HttpDelete("all-images")]
    public async Task<IActionResult> DeleteUserImage()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var isDeleted = await _userManagementService.DeleteAllUserImagesAsync(userId);

        return isDeleted
            ? Ok(new { Message = "All images deleted successfully." })
            : BadRequest(new { Message = "Failed to delete images." });
    }

    [HttpPut("details")]
    public async Task<IActionResult> UpdateUserDetails([FromBody] UserDetailsRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var userDetailsDto = _mapper.Map<UserDetailsDTO>(request);
        userDetailsDto.Id = userId;

        await _userManagementService.UpdateUserDetailsAsync(userDetailsDto);

        return Ok(new { Message = "User details updated successfully." });
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangeUserPassword([FromBody] ChangePasswordRequest request)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        await _userService.ChangeUserPasswordAsync(userId, request.CurrentPassword, request.NewPassword);

        return Ok(new { Message = "Password changed successfully." });
    }
}
