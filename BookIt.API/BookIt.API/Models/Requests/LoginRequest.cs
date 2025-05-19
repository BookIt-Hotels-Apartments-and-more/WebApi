<<<<<<<< HEAD:BookIt.API/BookIt.BLL/DTOs/Request/LoginRequest.cs
namespace BookIt.BLL.DTOs;
========
namespace BookIt.API.Models.Requests;
>>>>>>>> main:BookIt.API/BookIt.API/Models/Requests/LoginRequest.cs

public record LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
