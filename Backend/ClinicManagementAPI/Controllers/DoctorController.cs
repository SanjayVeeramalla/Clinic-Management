using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManagementAPI.DTOs.Doctor;
using ClinicManagementAPI.Services.Interfaces;

namespace ClinicManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorController(IDoctorService doctorService) => _doctorService = doctorService;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] bool? isAvailable, [FromQuery] int? specializationId)
    {
        var result = await _doctorService.GetAllDoctorsAsync(isAvailable, specializationId);
        return Ok(result);
    }

    [HttpGet("{doctorId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int doctorId)
    {
        var result = await _doctorService.GetDoctorByIdAsync(doctorId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("profile")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _doctorService.GetDoctorByUserIdAsync(userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("{doctorId:int}")]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> Update(int doctorId, [FromBody] UpdateDoctorDto dto)
    {
        var result = await _doctorService.UpdateDoctorAsync(doctorId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{doctorId:int}/appointments")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> GetAppointments(int doctorId,
        [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, [FromQuery] int? statusId)
    {
        var result = await _doctorService.GetDoctorAppointmentsAsync(doctorId, fromDate, toDate, statusId);
        return Ok(result);
    }

    [HttpGet("{doctorId:int}/available-slots")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableSlots(int doctorId, [FromQuery] DateOnly date)
    {
        var result = await _doctorService.GetAvailableSlotsAsync(doctorId, date);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{doctorId:int}/schedule")]
    [Authorize(Roles = "Doctor,Admin")]
    public async Task<IActionResult> SetSchedule(int doctorId, [FromBody] DoctorScheduleDto dto)
    {
        var result = await _doctorService.SetScheduleAsync(doctorId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{appointmentId:int}/prescription")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> AddPrescription(int appointmentId, [FromBody] AddPrescriptionDto dto,
        [FromServices] IAppointmentService appointmentService)
    {
        var result = await appointmentService.AddPrescriptionAsync(appointmentId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{appointmentId:int}/status")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId,
        [FromBody] UpdateAppointmentStatusDto dto,
        [FromServices] IAppointmentService appointmentService)
    {
        var result = await appointmentService.UpdateAppointmentStatusAsync(appointmentId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}