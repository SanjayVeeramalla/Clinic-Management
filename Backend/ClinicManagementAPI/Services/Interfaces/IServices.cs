using ClinicManagementAPI.DTOs.Admin;
using ClinicManagementAPI.DTOs.Appointment;
using ClinicManagementAPI.DTOs.Auth;
using ClinicManagementAPI.DTOs.Doctor;
using ClinicManagementAPI.DTOs.Patient;
using ClinicManagementAPI.Helpers;

namespace ClinicManagementAPI.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto dto);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse> LogoutAsync(string refreshToken);
}

public interface IDoctorService
{
    Task<ApiResponse<DoctorResponseDto>> CreateDoctorAsync(CreateDoctorDto dto);
    Task<ApiResponse<List<DoctorResponseDto>>> GetAllDoctorsAsync(bool? isAvailable = null, int? specializationId = null);
    Task<ApiResponse<DoctorResponseDto>> GetDoctorByIdAsync(int doctorId);
    Task<ApiResponse<DoctorResponseDto>> GetDoctorByUserIdAsync(int userId);
    Task<ApiResponse> UpdateDoctorAsync(int doctorId, UpdateDoctorDto dto);
    Task<ApiResponse<List<AppointmentResponseDto>>> GetDoctorAppointmentsAsync(
        int doctorId, DateOnly? fromDate = null, DateOnly? toDate = null, int? statusId = null);
    Task<ApiResponse<AvailableSlotsResponseDto>> GetAvailableSlotsAsync(int doctorId, DateOnly date);
    Task<ApiResponse> SetScheduleAsync(int doctorId, DoctorScheduleDto dto);
}

public interface IPatientService
{
    Task<ApiResponse<PatientResponseDto>> GetPatientProfileAsync(int userId);
    Task<ApiResponse> UpdatePatientProfileAsync(int userId, UpdatePatientProfileDto dto);
    Task<ApiResponse<List<PatientResponseDto>>> GetAllPatientsAsync(string? searchTerm = null);
    Task<ApiResponse<PatientResponseDto>> GetPatientByIdAsync(int patientId);
}

public interface IAppointmentService
{
    Task<ApiResponse<AppointmentResponseDto>> BookAppointmentAsync(int userId, BookAppointmentDto dto);
    Task<ApiResponse<List<AppointmentResponseDto>>> GetPatientAppointmentsAsync(int userId, int? statusId = null);
    Task<ApiResponse<AppointmentResponseDto>> GetAppointmentByIdAsync(int appointmentId);
    Task<ApiResponse> CancelAppointmentAsync(int appointmentId, int userId, CancelAppointmentDto dto);
    Task<ApiResponse> UpdateAppointmentStatusAsync(int appointmentId, UpdateAppointmentStatusDto dto);
    Task<ApiResponse> AddPrescriptionAsync(int appointmentId, AddPrescriptionDto dto);
}

public interface IAdminService
{
    Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync();
    Task<ApiResponse<List<AppointmentSummaryDto>>> GetAppointmentSummaryReportAsync(ReportRequestDto dto);
    Task<ApiResponse<List<DoctorWorkloadDto>>> GetDoctorWorkloadReportAsync(ReportRequestDto dto);
    Task<ApiResponse> DeactivateUserAsync(int userId);
}