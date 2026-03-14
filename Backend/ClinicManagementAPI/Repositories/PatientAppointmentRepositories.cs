using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ClinicManagementAPI.Data;
using ClinicManagementAPI.DTOs.Appointment;
using ClinicManagementAPI.DTOs.Patient;
using ClinicManagementAPI.Repositories.Interfaces;

namespace ClinicManagementAPI.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly ClinicDbContext _context;

    public PatientRepository(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<(int PatientId, string Message)> CreatePatientAsync(int userId, UpdatePatientProfileDto? dto = null)
    {
        var patientIdParam = new SqlParameter("@PatientId", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        var messageParam = new SqlParameter("@Message", System.Data.SqlDbType.NVarChar, 200) { Direction = System.Data.ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_CreatePatient @UserId, @DateOfBirth, @Gender, @BloodGroup, @Address, @EmergencyContact, @PatientId OUTPUT, @Message OUTPUT",
            new SqlParameter("@UserId", userId),
            new SqlParameter("@DateOfBirth", (object?)dto?.DateOfBirth?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value),
            new SqlParameter("@Gender", (object?)dto?.Gender ?? DBNull.Value),
            new SqlParameter("@BloodGroup", (object?)dto?.BloodGroup ?? DBNull.Value),
            new SqlParameter("@Address", (object?)dto?.Address ?? DBNull.Value),
            new SqlParameter("@EmergencyContact", (object?)dto?.EmergencyContact ?? DBNull.Value),
            patientIdParam,
            messageParam
        );

        return ((int)patientIdParam.Value, (string)messageParam.Value);
    }

    public async Task<PatientResponseDto?> GetPatientByUserIdAsync(int userId)
    {
        var p = await _context.Patients
            .Include(p => p.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (p == null) return null;

        return MapToDto(p);
    }

    public async Task<PatientResponseDto?> GetPatientByIdAsync(int patientId)
    {
        var p = await _context.Patients
            .Include(p => p.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PatientId == patientId);

        if (p == null) return null;

        return MapToDto(p);
    }

    public async Task<string> UpdatePatientProfileAsync(int patientId, UpdatePatientProfileDto dto)
    {
        var messageParam = new SqlParameter("@Message", System.Data.SqlDbType.NVarChar, 200) { Direction = System.Data.ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_UpdatePatientProfile @PatientId, @FullName, @Phone, @DateOfBirth, @Gender, @BloodGroup, @Address, @EmergencyContact, @MedicalHistory, @Message OUTPUT",
            new SqlParameter("@PatientId", patientId),
            new SqlParameter("@FullName", dto.FullName),
            new SqlParameter("@Phone", (object?)dto.Phone ?? DBNull.Value),
            new SqlParameter("@DateOfBirth", (object?)dto.DateOfBirth?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value),
            new SqlParameter("@Gender", (object?)dto.Gender ?? DBNull.Value),
            new SqlParameter("@BloodGroup", (object?)dto.BloodGroup ?? DBNull.Value),
            new SqlParameter("@Address", (object?)dto.Address ?? DBNull.Value),
            new SqlParameter("@EmergencyContact", (object?)dto.EmergencyContact ?? DBNull.Value),
            new SqlParameter("@MedicalHistory", (object?)dto.MedicalHistory ?? DBNull.Value),
            messageParam
        );

        return (string)messageParam.Value;
    }

    public async Task<List<PatientResponseDto>> GetAllPatientsAsync(string? searchTerm = null)
    {
        var query = _context.Patients.Include(p => p.User).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p => p.User.FullName.Contains(searchTerm) || p.User.Email.Contains(searchTerm));

        var patients = await query.OrderBy(p => p.User.FullName).ToListAsync();
        return patients.Select(MapToDto).ToList();
    }

    private static PatientResponseDto MapToDto(Models.Patient p) => new()
    {
        PatientId = p.PatientId, UserId = p.UserId, FullName = p.User.FullName,
        Email = p.User.Email, Phone = p.User.Phone, DateOfBirth = p.DateOfBirth,
        Gender = p.Gender, BloodGroup = p.BloodGroup, Address = p.Address,
        EmergencyContact = p.EmergencyContact, MedicalHistory = p.MedicalHistory
    };
}

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ClinicDbContext _context;

    public AppointmentRepository(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<(int AppointmentId, string Message)> BookAppointmentAsync(int patientId, BookAppointmentDto dto)
    {
        var appointmentIdParam = new SqlParameter("@AppointmentId", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        var messageParam = new SqlParameter("@Message", System.Data.SqlDbType.NVarChar, 200) { Direction = System.Data.ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_BookAppointment @PatientId, @DoctorId, @AppointmentDate, @AppointmentTime, @ReasonForVisit, @AppointmentId OUTPUT, @Message OUTPUT",
            new SqlParameter("@PatientId", patientId),
            new SqlParameter("@DoctorId", dto.DoctorId),
            new SqlParameter("@AppointmentDate", dto.AppointmentDate.ToDateTime(TimeOnly.MinValue)),
            new SqlParameter("@AppointmentTime", dto.AppointmentTime.ToTimeSpan()),
            new SqlParameter("@ReasonForVisit", (object?)dto.ReasonForVisit ?? DBNull.Value),
            appointmentIdParam,
            messageParam
        );

        return ((int)appointmentIdParam.Value, (string)messageParam.Value);
    }

    public async Task<List<AppointmentResponseDto>> GetPatientAppointmentsAsync(int patientId, int? statusId = null)
    {
        var appointments = await _context.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .Include(a => a.Status)
            .Include(a => a.Prescription)
            .AsNoTracking()
            .Where(a => a.PatientId == patientId && (statusId == null || a.StatusId == statusId))
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        return appointments.Select(MapToDto).ToList();
    }

    public async Task<AppointmentResponseDto?> GetAppointmentByIdAsync(int appointmentId)
    {
        var a = await _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .Include(a => a.Status)
            .Include(a => a.Prescription)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

        return a == null ? null : MapToDto(a);
    }

    public async Task<string> CancelAppointmentAsync(int appointmentId, int cancelledByUserId, string reason)
    {
        var messageParam = new SqlParameter("@Message", System.Data.SqlDbType.NVarChar, 200) { Direction = System.Data.ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_CancelAppointment @AppointmentId, @CancelledByUserId, @CancellationReason, @Message OUTPUT",
            new SqlParameter("@AppointmentId", appointmentId),
            new SqlParameter("@CancelledByUserId", cancelledByUserId),
            new SqlParameter("@CancellationReason", reason),
            messageParam
        );

        return (string)messageParam.Value;
    }

    public async Task<string> UpdateAppointmentStatusAsync(int appointmentId, string newStatus, string? notes = null)
    {
        var messageParam = new SqlParameter("@Message", System.Data.SqlDbType.NVarChar, 200) { Direction = System.Data.ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_UpdateAppointmentStatus @AppointmentId, @NewStatusName, @Notes, @Message OUTPUT",
            new SqlParameter("@AppointmentId", appointmentId),
            new SqlParameter("@NewStatusName", newStatus),
            new SqlParameter("@Notes", (object?)notes ?? DBNull.Value),
            messageParam
        );

        return (string)messageParam.Value;
    }

    public async Task<(int PrescriptionId, string Message)> AddPrescriptionAsync(int appointmentId, DTOs.Doctor.AddPrescriptionDto dto)
    {
        var prescriptionIdParam = new SqlParameter("@PrescriptionId", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        var messageParam = new SqlParameter("@Message", System.Data.SqlDbType.NVarChar, 200) { Direction = System.Data.ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_AddPrescription @AppointmentId, @Diagnosis, @Medications, @Instructions, @FollowUpDate, @PrescriptionId OUTPUT, @Message OUTPUT",
            new SqlParameter("@AppointmentId", appointmentId),
            new SqlParameter("@Diagnosis", dto.Diagnosis),
            new SqlParameter("@Medications", dto.Medications),
            new SqlParameter("@Instructions", (object?)dto.Instructions ?? DBNull.Value),
            new SqlParameter("@FollowUpDate", (object?)dto.FollowUpDate?.ToDateTime(TimeOnly.MinValue) ?? DBNull.Value),
            prescriptionIdParam,
            messageParam
        );

        return ((int)prescriptionIdParam.Value, (string)messageParam.Value);
    }

    private static AppointmentResponseDto MapToDto(Models.Appointment a) => new()
    {
        AppointmentId = a.AppointmentId,
        AppointmentDate = a.AppointmentDate,
        AppointmentTime = a.AppointmentTime,
        PatientId = a.PatientId,
        PatientName = a.Patient?.User?.FullName ?? string.Empty,
        PatientPhone = a.Patient?.User?.Phone,
        DoctorId = a.DoctorId,
        DoctorName = a.Doctor?.User?.FullName ?? string.Empty,
        Specialization = a.Doctor?.Specialization?.Name ?? string.Empty,
        Status = a.Status?.StatusName ?? string.Empty,
        ReasonForVisit = a.ReasonForVisit,
        Notes = a.Notes,
        CancellationReason = a.CancellationReason,
        Prescription = a.Prescription == null ? null : new DTOs.Appointment.PrescriptionDto
        {
            PrescriptionId = a.Prescription.PrescriptionId,
            Diagnosis = a.Prescription.Diagnosis,
            Medications = a.Prescription.Medications,
            Instructions = a.Prescription.Instructions,
            FollowUpDate = a.Prescription.FollowUpDate
        }
    };
}