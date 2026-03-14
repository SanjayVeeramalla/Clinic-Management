using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManagement.API.DTOs.Auth;
using ClinicManagement.API.Services.Interfaces;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>Register a new user (Patient or Doctor)</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Login and receive JWT tokens</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>Refresh access token using refresh token</summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>Logout and revoke refresh token</summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _authService.LogoutAsync(dto.RefreshToken);
        return Ok(result);
    }
}
