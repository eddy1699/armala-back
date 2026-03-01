using MediatR;
using Microsoft.AspNetCore.Mvc;
using Armala.Auth.Application.Commands;
using Armala.Auth.Application.DTOs;

namespace Armala.Auth.Interfaces.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtpController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OtpController> _logger;

    public OtpController(IMediator mediator, ILogger<OtpController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Enviar código OTP al email del usuario para verificar su cuenta
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(SendOtpResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequestDto request)
    {
        try
        {
            var command = new SendOtpCommand { Email = request.Email };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al enviar OTP para: {Email}", request.Email);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Verificar el código OTP para activar la cuenta del usuario
    /// </summary>
    [HttpPost("verify")]
    [ProducesResponseType(typeof(VerifyOtpResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
    {
        try
        {
            var command = new VerifyOtpCommand
            {
                Email = request.Email,
                Code = request.Code
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error al verificar OTP para: {Email}", request.Email);
            return BadRequest(new { error = ex.Message });
        }
    }
}
