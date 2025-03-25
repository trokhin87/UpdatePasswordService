using DTO;
using Inteerfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using WebLevel.Example;

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

    /// <summary>
    /// Запрос на сброс пароля
    /// </summary>
    [HttpPost("request-reset-password")]
    [SwaggerOperation(Summary = "Запрос на сброс пароля", Description = "Отправляет ссылку для сброса пароля на указанный email.")]
    [SwaggerRequestExample(typeof(string), typeof(RequestResetPasswordExample))]
    [SwaggerResponse(200, "Ссылка для сброса пароля отправлена", typeof(object))]
    [SwaggerResponse(400, "Пользователь не существует или отправка письма не удалась", typeof(object))]
    public async Task<IActionResult> RequestTaskAsync([FromBody] string email)
    {
        var success = await _service.GenerationTokenAsync(email);
        if (!success)
        {
            return BadRequest(new { message = "User does not exist or email sending failed" });
        }

        return Ok(new { message = "Reset link sent to email" });
    }

    /// <summary>
    /// Сброс пароля
    /// </summary>
    [HttpPost("reset-password")]
    [SwaggerOperation(Summary = "Сброс пароля", Description = "Позволяет пользователю сбросить пароль с использованием токена.")]
    [SwaggerRequestExample(typeof(PasswordResetDto), typeof(ResetPasswordExample))]
    [SwaggerResponse(200, "Пароль успешно обновлен", typeof(object))]
    [SwaggerResponse(400, "Неверный токен или сброс пароля не удался", typeof(object))]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] PasswordResetDto resetDto)
    {
        var success = await _service.CheckPasswordAsync(resetDto);
        if (!success)
        {
            return BadRequest(new { message = "Invalid token or password update failed" });
        }

        return Ok(new { message = "Password updated successfully" });
    }
}