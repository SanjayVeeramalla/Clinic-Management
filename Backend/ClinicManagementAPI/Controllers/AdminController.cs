using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicManagementAPI.DTOs;
using ClinicManagementAPI.DTOs.Doctor;
using ClinicManagementAPI.Services.Interfaces;
using ClinicManagementAPI.DTOs.Admin;


namespace ClinicManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientService _patientService;

    public AdminController(IAdminService adminService, IDoctorService doctorService, IPatientService patientService)
    {
        _adminService = adminService;
        _doctorService = doctorService;
        _patientService = patientService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _adminService.GetDashboardStatsAsync();
        return Ok(result);
    }

    [HttpGet("doctors")]
    public async Task<IActionResult> GetAllDoctors()
    {
        var result = await _doctorService.GetAllDoctorsAsync();
        return Ok(result);
    }

    [HttpPost("doctors")]
    public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorDto dto)
    {
        var result = await _doctorService.CreateDoctorAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("patients")]
    public async Task<IActionResult> GetAllPatients([FromQuery] string? search)
    {
        var result = await _patientService.GetAllPatientsAsync(search);
        return Ok(result);
    }

    [HttpGet("patients/{patientId:int}")]
    public async Task<IActionResult> GetPatient(int patientId)
    {
        var result = await _patientService.GetPatientByIdAsync(patientId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPut("users/{userId:int}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int userId)
    {
        var result = await _adminService.DeactivateUserAsync(userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("reports/appointments")]
    public async Task<IActionResult> AppointmentSummaryReport([FromBody] ReportRequestDto dto)
    {
        var result = await _adminService.GetAppointmentSummaryReportAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("reports/doctor-workload")]
    public async Task<IActionResult> DoctorWorkloadReport([FromBody] ReportRequestDto dto)
    {
        var result = await _adminService.GetDoctorWorkloadReportAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}