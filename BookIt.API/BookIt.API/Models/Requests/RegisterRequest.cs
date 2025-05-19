<<<<<<<< HEAD:BookIt.API/BookIt.BLL/DTOs/Request/RegisterRequest.cs
namespace BookIt.BLL.DTOs;

public class RegisterRequest
========
namespace BookIt.API.Models.Requests;

public record RegisterRequest
>>>>>>>> main:BookIt.API/BookIt.API/Models/Requests/RegisterRequest.cs
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}