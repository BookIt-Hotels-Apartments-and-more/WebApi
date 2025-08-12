using AutoMapper;
using BookIt.API.Models.Requests;
using BookIt.API.Models.Responses;
using BookIt.BLL.DTOs;
using BookIt.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookIt.API.Controllers;

[ApiController]
[Route("user")]
public class UserManagementController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IUserManagementService _userManagementService;

    public UserManagementController(IMapper mapper, IUserManagementService userManagementService)
    {
        _mapper = mapper;
        _userManagementService = userManagementService;
    }

    [HttpPut("images")]
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

    [HttpGet("images")]
    [Authorize]
    public async Task<IActionResult> GetUserImages()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int userId = int.Parse(userIdStr!);

        var imageDtos = await _userManagementService.GetUserImagesAsync(userId);
        var imagesResponse = _mapper.Map<IEnumerable<ImageResponse>>(imageDtos);

        return Ok(imagesResponse);
    }

    [HttpDelete("all-images")]
    [Authorize]
    public async Task<IActionResult> DeleteUserImage()
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int userId = int.Parse(userIdStr!);

        var isDeleted = await _userManagementService.DeleteAllUserImagesAsync(userId);

        return isDeleted
            ? Ok(new { Message = "All images deleted successfully." })
            : BadRequest(new { Message = "Failed to delete images." });
    }

    [HttpPut("details")]
    [Authorize]
    public async Task<IActionResult> UpdateUserDetails([FromBody] UserDetailsRequest request)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int userId = int.Parse(userIdStr!);

        var userDetailsDto = _mapper.Map<UserDetailsDTO>(request);
        userDetailsDto.Id = userId;

        await _userManagementService.UpdateUserDetailsAsync(userDetailsDto);

        return Ok(new { Message = "User details updated successfully." });
    }
}
