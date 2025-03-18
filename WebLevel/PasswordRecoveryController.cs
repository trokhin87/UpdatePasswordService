using DTO;
using Inteerfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebLevel;


[ApiController]
[Route("api/passwordrecovery")]
public class PasswordRecoveryController : ControllerBase
{
    private readonly IService _service;

    public PasswordRecoveryController(IService service)
    {
        _service = service;
    }

    [HttpPost("request-reset-password")]
    public async Task<IActionResult> RequestTaskAsync([FromBody] string email)
    {
        var success = await _service.GenerationTokenAsync(email);
        if (!success)
        {
            return BadRequest(new { message = "User does not exist or email sending failed" });
        }

        return Ok(new { message = "Reset link sent to email" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] PasswordResetDto resetDto)
    {
        var success=await _service.CheckPasswordAsync(resetDto);
        if (!success)
        {
            return BadRequest(new { message = "Invalid token or password update failed" });
        }

        return Ok(new { message = "Password updated successfully" });
    }
}