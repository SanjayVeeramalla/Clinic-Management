using ClinicManagementAPI.DTOs.Auth;
using ClinicManagementAPI.DTOs.Doctor;
using ClinicManagementAPI.DTOs.Patient;
using ClinicManagementAPI.DTOs.Appointment;
using ClinicManagementAPI.DTOs.Admin;

namespace ClinicManagementAPI.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<(int UserId, string Message)> RegisterUserAsync(RegisterRequestDto dto, string passwordHash);
    Task<UserInfoDto?> GetUserByEmailAsync(string email);
    Task<UserInfoDto?> GetUserByIdAsync(int userId);
    Task<string?> GetPasswordHashAsync(string email);
    Task SaveRefreshTokenAsync(int userId, string token, DateTime expiresAt);
    Task<(int UserId, string Email, string Role, DateTime ExpiresAt, bool IsRevoked)?>
        GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
}

public interface IDoctorRepository
{
    Task<(int DoctorId, string Message)> CreateDoctorAsync(CreateDoctorDto dto);
    Task<List<DoctorResponseDto>> GetAllDoctorsAsync(bool? isAvailable = null, int? specializationId = null);
    Task<DoctorResponseDto?> GetDoctorByIdAsync(int doctorId);
    Task<DoctorResponseDto?> GetDoctorByUserIdAsync(int userId);
    Task<string> UpdateDoctorAsync(int doctorId, UpdateDoctorDto dto);
    Task<List<AppointmentResponseDto>> GetDoctorAppointmentsAsync(int doctorId,
        DateOnly? fromDate = null, DateOnly? toDate = null, int? statusId = null);
    Task<AvailableSlotsResponseDto> GetAvailableSlotsAsync(int doctorId, DateOnly date);
    Task<string> SetScheduleAsync(int doctorId, DoctorScheduleDto dto);
}

public interface IPatientRepository
{
    Task<(int PatientId, string Message)> CreatePatientAsync(int userId, UpdatePatientProfileDto? dto = null);
    Task<PatientResponseDto?> GetPatientByUserIdAsync(int userId);
    Task<PatientResponseDto?> GetPatientByIdAsync(int patientId);
    Task<string> UpdatePatientProfileAsync(int patientId, UpdatePatientProfileDto dto);
    Task<List<PatientResponseDto>> GetAllPatientsAsync(string? searchTerm = null);
}

public interface IAppointmentRepository
{
    Task<(int AppointmentId, string Message)> BookAppointmentAsync(int patientId, BookAppointmentDto dto);
    Task<List<AppointmentResponseDto>> GetPatientAppointmentsAsync(int patientId, int? statusId = null);
    Task<AppointmentResponseDto?> GetAppointmentByIdAsync(int appointmentId);
    Task<string> CancelAppointmentAsync(int appointmentId, int cancelledByUserId, string reason);
    Task<string> UpdateAppointmentStatusAsync(int appointmentId, string newStatus, string? notes = null);
    Task<(int PrescriptionId, string Message)> AddPrescriptionAsync(int appointmentId, AddPrescriptionDto dto);
}

public interface IAdminRepository
{
    Task<DashboardStatsDto> GetDashboardStatsAsync();
    Task<List<AppointmentSummaryDto>> GetAppointmentSummaryReportAsync(DateOnly fromDate, DateOnly toDate);
    Task<List<DoctorWorkloadDto>> GetDoctorWorkloadReportAsync(DateOnly fromDate, DateOnly toDate);
    Task<string> DeactivateUserAsync(int userId);
}