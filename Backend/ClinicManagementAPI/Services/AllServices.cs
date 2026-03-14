using BCrypt.Net;
using ClinicManagementAPI.DTOs.Admin;
using ClinicManagementAPI.DTOs.Appointment;
using ClinicManagementAPI.DTOs.Auth;
using ClinicManagementAPI.DTOs.Doctor;
using ClinicManagementAPI.DTOs.Patient;
using ClinicManagementAPI.Helpers;
using ClinicManagementAPI.Repositories.Interfaces;
using ClinicManagementAPI.Services.Interfaces;

namespace ClinicManagementAPI.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepo;
    private readonly IPatientRepository _patientRepo;
    private readonly JwtHelper _jwtHelper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IAuthRepository authRepo, IPatientRepository patientRepo,
        JwtHelper jwtHelper, ILogger<AuthService> logger)
    {
        _authRepo = authRepo;
        _patientRepo = patientRepo;
        _jwtHelper = jwtHelper;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto)
    {
        try
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var (userId, message) = await _authRepo.RegisterUserAsync(dto, passwordHash);

            if (userId <= 0)
                return ApiResponse<AuthResponseDto>.Fail(message);

            // Auto-create patient profile if role is Patient
            if (dto.Role == "Patient")
                await _patientRepo.CreatePatientAsync(userId);

            var user = await _authRepo.GetUserByIdAsync(userId);
            var accessToken = _jwtHelper.GenerateAccessToken(userId, dto.Email, dto.Role);
            var refreshToken = _jwtHelper.GenerateRefreshToken();
            var refreshExpiry = _jwtHelper.GetRefreshTokenExpiry();

            await _authRepo.SaveRefreshTokenAsync(userId, refreshToken, refreshExpiry);

            return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = user!
            }, "Registration successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", dto.Email);
            return ApiResponse<AuthResponseDto>.Fail("Registration failed. Please try again.");
        }
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto dto)
    {
        try
        {
            var passwordHash = await _authRepo.GetPasswordHashAsync(dto.Email);
            if (passwordHash == null)
                return ApiResponse<AuthResponseDto>.Fail("Invalid email or password");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, passwordHash))
                return ApiResponse<AuthResponseDto>.Fail("Invalid email or password");

            var user = await _authRepo.GetUserByEmailAsync(dto.Email);
            if (user == null)
                return ApiResponse<AuthResponseDto>.Fail("User not found");

            var accessToken = _jwtHelper.GenerateAccessToken(user.UserId, user.Email, user.Role);
            var refreshToken = _jwtHelper.GenerateRefreshToken();
            var refreshExpiry = _jwtHelper.GetRefreshTokenExpiry();

            await _authRepo.SaveRefreshTokenAsync(user.UserId, refreshToken, refreshExpiry);

            return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = user
            }, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for {Email}", dto.Email);
            return ApiResponse<AuthResponseDto>.Fail("Login failed. Please try again.");
        }
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        var tokenData = await _authRepo.GetRefreshTokenAsync(refreshToken);
        if (tokenData == null || tokenData.Value.IsRevoked || tokenData.Value.ExpiresAt < DateTime.UtcNow)
            return ApiResponse<AuthResponseDto>.Fail("Invalid or expired refresh token");

        await _authRepo.RevokeRefreshTokenAsync(refreshToken);

        var newAccessToken = _jwtHelper.GenerateAccessToken(
            tokenData.Value.UserId, tokenData.Value.Email, tokenData.Value.Role);
        var newRefreshToken = _jwtHelper.GenerateRefreshToken();
        var newExpiry = _jwtHelper.GetRefreshTokenExpiry();

        await _authRepo.SaveRefreshTokenAsync(tokenData.Value.UserId, newRefreshToken, newExpiry);

        var user = await _authRepo.GetUserByIdAsync(tokenData.Value.UserId);

        return ApiResponse<AuthResponseDto>.Ok(new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = user!
        });
    }

    public async Task<ApiResponse> LogoutAsync(string refreshToken)
    {
        await _authRepo.RevokeRefreshTokenAsync(refreshToken);
        return ApiResponse.Ok("Logged out successfully");
    }
}

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepo;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(IDoctorRepository doctorRepo, ILogger<DoctorService> logger)
    {
        _doctorRepo = doctorRepo;
        _logger = logger;
    }

    public async Task<ApiResponse<DoctorResponseDto>> CreateDoctorAsync(CreateDoctorDto dto)
    {
        var (doctorId, message) = await _doctorRepo.CreateDoctorAsync(dto);
        if (doctorId <= 0) return ApiResponse<DoctorResponseDto>.Fail(message);

        var doctor = await _doctorRepo.GetDoctorByIdAsync(doctorId);
        return ApiResponse<DoctorResponseDto>.Ok(doctor!, message);
    }

    public async Task<ApiResponse<List<DoctorResponseDto>>> GetAllDoctorsAsync(bool? isAvailable = null, int? specializationId = null)
    {
        var doctors = await _doctorRepo.GetAllDoctorsAsync(isAvailable, specializationId);
        return ApiResponse<List<DoctorResponseDto>>.Ok(doctors);
    }

    public async Task<ApiResponse<DoctorResponseDto>> GetDoctorByIdAsync(int doctorId)
    {
        var doctor = await _doctorRepo.GetDoctorByIdAsync(doctorId);
        if (doctor == null) return ApiResponse<DoctorResponseDto>.Fail("Doctor not found");
        return ApiResponse<DoctorResponseDto>.Ok(doctor);
    }

    public async Task<ApiResponse<DoctorResponseDto>> GetDoctorByUserIdAsync(int userId)
    {
        var doctor = await _doctorRepo.GetDoctorByUserIdAsync(userId);
        if (doctor == null) return ApiResponse<DoctorResponseDto>.Fail("Doctor profile not found");
        return ApiResponse<DoctorResponseDto>.Ok(doctor);
    }

    public async Task<ApiResponse> UpdateDoctorAsync(int doctorId, UpdateDoctorDto dto)
    {
        var message = await _doctorRepo.UpdateDoctorAsync(doctorId, dto);
        if (message.Contains("not found") || message.Contains("error"))
            return ApiResponse.Fail(message);
        return ApiResponse.Ok(message);
    }

    public async Task<ApiResponse<List<AppointmentResponseDto>>> GetDoctorAppointmentsAsync(
        int doctorId, DateOnly? fromDate = null, DateOnly? toDate = null, int? statusId = null)
    {
        var appointments = await _doctorRepo.GetDoctorAppointmentsAsync(doctorId, fromDate, toDate, statusId);
        return ApiResponse<List<AppointmentResponseDto>>.Ok(appointments);
    }

    public async Task<ApiResponse<AvailableSlotsResponseDto>> GetAvailableSlotsAsync(int doctorId, DateOnly date)
    {
        if (date < DateOnly.FromDateTime(DateTime.UtcNow))
            return ApiResponse<AvailableSlotsResponseDto>.Fail("Cannot check slots for past dates");

        var slots = await _doctorRepo.GetAvailableSlotsAsync(doctorId, date);
        return ApiResponse<AvailableSlotsResponseDto>.Ok(slots);
    }

    public async Task<ApiResponse> SetScheduleAsync(int doctorId, DoctorScheduleDto dto)
    {
        var message = await _doctorRepo.SetScheduleAsync(doctorId, dto);
        return message.Contains("success") ? ApiResponse.Ok(message) : ApiResponse.Fail(message);
    }
}

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepo;

    public PatientService(IPatientRepository patientRepo)
    {
        _patientRepo = patientRepo;
    }

    public async Task<ApiResponse<PatientResponseDto>> GetPatientProfileAsync(int userId)
    {
        var patient = await _patientRepo.GetPatientByUserIdAsync(userId);
        if (patient == null) return ApiResponse<PatientResponseDto>.Fail("Patient profile not found");
        return ApiResponse<PatientResponseDto>.Ok(patient);
    }

    public async Task<ApiResponse> UpdatePatientProfileAsync(int userId, UpdatePatientProfileDto dto)
    {
        var patient = await _patientRepo.GetPatientByUserIdAsync(userId);
        if (patient == null) return ApiResponse.Fail("Patient profile not found");

        var message = await _patientRepo.UpdatePatientProfileAsync(patient.PatientId, dto);
        return message.Contains("success") ? ApiResponse.Ok(message) : ApiResponse.Fail(message);
    }

    public async Task<ApiResponse<List<PatientResponseDto>>> GetAllPatientsAsync(string? searchTerm = null)
    {
        var patients = await _patientRepo.GetAllPatientsAsync(searchTerm);
        return ApiResponse<List<PatientResponseDto>>.Ok(patients);
    }

    public async Task<ApiResponse<PatientResponseDto>> GetPatientByIdAsync(int patientId)
    {
        var patient = await _patientRepo.GetPatientByIdAsync(patientId);
        if (patient == null) return ApiResponse<PatientResponseDto>.Fail("Patient not found");
        return ApiResponse<PatientResponseDto>.Ok(patient);
    }
}

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IPatientRepository _patientRepo;

    public AppointmentService(IAppointmentRepository appointmentRepo, IPatientRepository patientRepo)
    {
        _appointmentRepo = appointmentRepo;
        _patientRepo = patientRepo;
    }

    public async Task<ApiResponse<AppointmentResponseDto>> BookAppointmentAsync(int userId, BookAppointmentDto dto)
    {
        var patient = await _patientRepo.GetPatientByUserIdAsync(userId);
        if (patient == null) return ApiResponse<AppointmentResponseDto>.Fail("Patient profile not found");

        var (appointmentId, message) = await _appointmentRepo.BookAppointmentAsync(patient.PatientId, dto);
        if (appointmentId <= 0) return ApiResponse<AppointmentResponseDto>.Fail(message);

        var appointment = await _appointmentRepo.GetAppointmentByIdAsync(appointmentId);
        return ApiResponse<AppointmentResponseDto>.Ok(appointment!, message);
    }

    public async Task<ApiResponse<List<AppointmentResponseDto>>> GetPatientAppointmentsAsync(int userId, int? statusId = null)
    {
        var patient = await _patientRepo.GetPatientByUserIdAsync(userId);
        if (patient == null) return ApiResponse<List<AppointmentResponseDto>>.Fail("Patient profile not found");

        var appointments = await _appointmentRepo.GetPatientAppointmentsAsync(patient.PatientId, statusId);
        return ApiResponse<List<AppointmentResponseDto>>.Ok(appointments);
    }

    public async Task<ApiResponse<AppointmentResponseDto>> GetAppointmentByIdAsync(int appointmentId)
    {
        var appointment = await _appointmentRepo.GetAppointmentByIdAsync(appointmentId);
        if (appointment == null) return ApiResponse<AppointmentResponseDto>.Fail("Appointment not found");
        return ApiResponse<AppointmentResponseDto>.Ok(appointment);
    }

    public async Task<ApiResponse> CancelAppointmentAsync(int appointmentId, int userId, CancelAppointmentDto dto)
    {
        var message = await _appointmentRepo.CancelAppointmentAsync(appointmentId, userId, dto.CancellationReason);
        return message.Contains("success") ? ApiResponse.Ok(message) : ApiResponse.Fail(message);
    }

    public async Task<ApiResponse> UpdateAppointmentStatusAsync(int appointmentId, UpdateAppointmentStatusDto dto)
    {
        var message = await _appointmentRepo.UpdateAppointmentStatusAsync(appointmentId, dto.Status, dto.Notes);
        return message.Contains("success") ? ApiResponse.Ok(message) : ApiResponse.Fail(message);
    }

    public async Task<ApiResponse> AddPrescriptionAsync(int appointmentId, AddPrescriptionDto dto)
    {
        var (prescriptionId, message) = await _appointmentRepo.AddPrescriptionAsync(appointmentId, dto);
        return prescriptionId > 0 ? ApiResponse.Ok(message) : ApiResponse.Fail(message);
    }
}

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepo;

    public AdminService(IAdminRepository adminRepo)
    {
        _adminRepo = adminRepo;
    }

    public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync()
    {
        var stats = await _adminRepo.GetDashboardStatsAsync();
        return ApiResponse<DashboardStatsDto>.Ok(stats);
    }

    public async Task<ApiResponse<List<AppointmentSummaryDto>>> GetAppointmentSummaryReportAsync(ReportRequestDto dto)
    {
        if (dto.FromDate > dto.ToDate) return ApiResponse<List<AppointmentSummaryDto>>.Fail("FromDate must be before ToDate");
        var result = await _adminRepo.GetAppointmentSummaryReportAsync(dto.FromDate, dto.ToDate);
        return ApiResponse<List<AppointmentSummaryDto>>.Ok(result);
    }

    public async Task<ApiResponse<List<DoctorWorkloadDto>>> GetDoctorWorkloadReportAsync(ReportRequestDto dto)
    {
        if (dto.FromDate > dto.ToDate) return ApiResponse<List<DoctorWorkloadDto>>.Fail("FromDate must be before ToDate");
        var result = await _adminRepo.GetDoctorWorkloadReportAsync(dto.FromDate, dto.ToDate);
        return ApiResponse<List<DoctorWorkloadDto>>.Ok(result);
    }

    public async Task<ApiResponse> DeactivateUserAsync(int userId)
    {
        var message = await _adminRepo.DeactivateUserAsync(userId);
        return message.Contains("success") ? ApiResponse.Ok(message) : ApiResponse.Fail(message);
    }
}