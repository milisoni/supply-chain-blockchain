using Microsoft.AspNetCore.Mvc;
using SupplyChainApi.DTOs;
using SupplyChainApi.Services;

namespace SupplyChainApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>Register a new user (Supplier, Manufacturer, Transporter, Distributor, Retailer, or Admin).</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password required.");
        var result = await _auth.RegisterAsync(request, ct);
        if (result == null)
            return BadRequest("Registration failed. Email may already be in use.");
        return Ok(result);
    }

    /// <summary>Login with email and password. Returns JWT and user info.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(request, ct);
        if (result == null)
            return Unauthorized("Invalid email or password.");
        return Ok(result);
    }
}
