using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManagement.API.DTOs.Admin;
using ClinicManagement.API.DTOs.Admin;
using ClinicManagement.API.DTOs.Doctor;
using ClinicManagement.API.Services.Interfaces;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;

    public AdminController(
        IAdminService adminService,
        IDoctorService doctorService,
        IPatientService patientService)
    {
        _adminService = adminService;
        _doctorService = doctorService;
        _patientService = patientService;
    }

    /// <summary>Get system overview stats</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _adminService.GetDashboardStatsAsync();
        return Ok(result);
    }

    // ─── Doctors ────────────────────────────────────────────

    /// <summary>List all doctors</summary>
    [HttpGet("doctors")]
    public async Task<IActionResult> GetAllDoctors()
    {
        var result = await _doctorService.GetAllDoctorsAsync();
        return Ok(result);
    }

    /// <summary>
    /// Create a full Doctor account (User + Doctor profile) in one step.
    /// Only Admin can create doctors — doctors cannot self-register.
    /// </summary>
    [HttpPost("doctors/create-account")]
    public async Task<IActionResult> CreateDoctorAccount([FromBody] CreateDoctorAccountDto dto)
    {
        var result = await _adminService.CreateDoctorAccountAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update a doctor's details</summary>
    [HttpPut("doctors/{doctorId:int}")]
    public async Task<IActionResult> UpdateDoctor(int doctorId, [FromBody] UpdateDoctorDto dto)
    {
        var result = await _doctorService.UpdateDoctorAsync(doctorId, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ─── Patients ────────────────────────────────────────────

    /// <summary>List all patients, with optional search</summary>
    [HttpGet("patients")]
    public async Task<IActionResult> GetAllPatients([FromQuery] string? search)
    {
        var result = await _patientService.GetAllPatientsAsync(search);
        return Ok(result);
    }

    /// <summary>Get a single patient's full details</summary>
    [HttpGet("patients/{patientId:int}")]
    public async Task<IActionResult> GetPatient(int patientId)
    {
        var result = await _patientService.GetPatientByIdAsync(patientId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // ─── User management ─────────────────────────────────────

    /// <summary>Deactivate a user (and cancel their future appointments if a doctor)</summary>
    [HttpPut("users/{userId:int}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int userId)
    {
        var result = await _adminService.DeactivateUserAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ─── Reports ─────────────────────────────────────────────

    /// <summary>Appointment status summary for a date range</summary>
    [HttpPost("reports/appointments")]
    public async Task<IActionResult> AppointmentSummaryReport([FromBody] ReportRequestDto dto)
    {
        var result = await _adminService.GetAppointmentSummaryReportAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Doctor workload report for a date range</summary>
    [HttpPost("reports/doctor-workload")]
    public async Task<IActionResult> DoctorWorkloadReport([FromBody] ReportRequestDto dto)
    {
        var result = await _adminService.GetDoctorWorkloadReportAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
