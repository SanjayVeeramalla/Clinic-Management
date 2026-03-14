using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManagementAPI.DTOs.Appointment;
using ClinicManagementAPI.DTOs.Patient;
using ClinicManagementAPI.Services.Interfaces;

namespace ClinicManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IAppointmentService _appointmentService;

    public PatientController(IPatientService patientService, IAppointmentService appointmentService)
    {
        _patientService = patientService;
        _appointmentService = appointmentService;
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _patientService.GetPatientProfileAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("profile")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdatePatientProfileDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _patientService.UpdatePatientProfileAsync(userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("appointments")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _appointmentService.BookAppointmentAsync(userId, dto);
        return result.Success ? CreatedAtAction(nameof(GetAppointment),
            new { appointmentId = result.Data?.AppointmentId }, result) : BadRequest(result);
    }

    [HttpGet("appointments")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetMyAppointments([FromQuery] int? statusId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _appointmentService.GetPatientAppointmentsAsync(userId, statusId);
        return Ok(result);
    }

    [HttpGet("appointments/{appointmentId:int}")]
    public async Task<IActionResult> GetAppointment(int appointmentId)
    {
        var result = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete("appointments/{appointmentId:int}")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> CancelAppointment(int appointmentId, [FromBody] CancelAppointmentDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _appointmentService.CancelAppointmentAsync(appointmentId, userId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}