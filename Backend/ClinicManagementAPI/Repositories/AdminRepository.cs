using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ClinicManagement.API.Data;
using ClinicManagement.API.DTOs.Admin;
using ClinicManagement.API.DTOs.Auth;
using ClinicManagement.API.Repositories.Interfaces;

namespace ClinicManagement.API.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly ClinicDbContext _context;

    public AdminRepository(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var totalPatients = await _context.Patients.Include(p => p.User).CountAsync(p => p.User.IsActive);
        var totalDoctors = await _context.Doctors.Include(d => d.User).CountAsync(d => d.User.IsActive);
        var totalAppointments = await _context.Appointments.CountAsync();
        var pendingAppointments = await _context.Appointments
            .Include(a => a.Status).CountAsync(a => a.Status.StatusName == "Pending");
        var completedAppointments = await _context.Appointments
            .Include(a => a.Status).CountAsync(a => a.Status.StatusName == "Completed");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayAppointments = await _context.Appointments.CountAsync(a => a.AppointmentDate == today);

        return new DashboardStatsDto
        {
            TotalPatients = totalPatients,
            TotalDoctors = totalDoctors,
            TotalAppointments = totalAppointments,
            PendingAppointments = pendingAppointments,
            CompletedAppointments = completedAppointments,
            TodayAppointments = todayAppointments
        };
    }

    public async Task<List<AppointmentSummaryDto>> GetAppointmentSummaryReportAsync(DateOnly fromDate, DateOnly toDate)
    {
        var fromDateTime = fromDate.ToDateTime(TimeOnly.MinValue);
        var toDateTime = toDate.ToDateTime(TimeOnly.MaxValue);

        var results = await _context.Database
            .SqlQueryRaw<AppointmentSummaryDto>(
                "EXEC sp_GetAppointmentSummaryReport @FromDate, @ToDate",
                new SqlParameter("@FromDate", fromDateTime),
                new SqlParameter("@ToDate", toDateTime))
            .ToListAsync();

        return results;
    }

    public async Task<List<DoctorWorkloadDto>> GetDoctorWorkloadReportAsync(DateOnly fromDate, DateOnly toDate)
    {
        var fromDateTime = fromDate.ToDateTime(TimeOnly.MinValue);
        var toDateTime = toDate.ToDateTime(TimeOnly.MaxValue);

        var results = await _context.Database
            .SqlQueryRaw<DoctorWorkloadDto>(
                "EXEC sp_GetDoctorWorkloadReport @FromDate, @ToDate",
                new SqlParameter("@FromDate", fromDateTime),
                new SqlParameter("@ToDate", toDateTime))
            .ToListAsync();

        return results;
    }

    public async Task<string> DeactivateUserAsync(int userId)
    {
        var messageParam = new SqlParameter("@Message", System.Data.SqlDbType.NVarChar, 200)
            { Direction = System.Data.ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_DeactivateUser @UserId, @Message OUTPUT",
            new SqlParameter("@UserId", userId),
            messageParam
        );

        return (string)messageParam.Value;
    }

    // Creates a User (Doctor role) + Doctor profile in a single SP call.
    // Admin provides all details — no separate registration step needed.
    public async Task<(int UserId, int DoctorId, string Message)> CreateDoctorAccountAsync(
        CreateDoctorAccountDto dto, string passwordHash)
    {
        var userIdParam    = new SqlParameter("@UserId",   System.Data.SqlDbType.Int)          { Direction = System.Data.ParameterDirection.Output };
        var doctorIdParam  = new SqlParameter("@DoctorId", System.Data.SqlDbType.Int)          { Direction = System.Data.ParameterDirection.Output };
        var messageParam   = new SqlParameter("@Message",  System.Data.SqlDbType.NVarChar, 200){ Direction = System.Data.ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_CreateDoctorAccount @FullName, @Email, @PasswordHash, @Phone, " +
            "@SpecializationId, @LicenseNumber, @YearsOfExperience, @ConsultationFee, " +
            "@UserId OUTPUT, @DoctorId OUTPUT, @Message OUTPUT",
            new SqlParameter("@FullName",           dto.FullName),
            new SqlParameter("@Email",              dto.Email),
            new SqlParameter("@PasswordHash",       passwordHash),
            new SqlParameter("@Phone",              (object?)dto.Phone ?? DBNull.Value),
            new SqlParameter("@SpecializationId",   dto.SpecializationId),
            new SqlParameter("@LicenseNumber",      dto.LicenseNumber),
            new SqlParameter("@YearsOfExperience",  dto.YearsOfExperience),
            new SqlParameter("@ConsultationFee",    dto.ConsultationFee),
            userIdParam,
            doctorIdParam,
            messageParam
        );

        return ((int)userIdParam.Value, (int)doctorIdParam.Value, (string)messageParam.Value);
    }
}
