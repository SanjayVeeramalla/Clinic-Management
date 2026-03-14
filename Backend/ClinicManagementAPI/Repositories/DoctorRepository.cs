using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ClinicManagement.API.Data;
using ClinicManagement.API.DTOs.Appointment;
using ClinicManagement.API.DTOs.Doctor;
using ClinicManagement.API.DTOs.Patient;
using ClinicManagement.API.Repositories.Interfaces;

namespace ClinicManagement.API.Repositories;

// ─────────────────────────────────────────────────────────────────────────────
// DoctorRepository
//
// Key design decisions after SP update:
//
// 1. CreateDoctorAsync REMOVED — doctor creation is now handled entirely by
//    AdminRepository.CreateDoctorAccountAsync → sp_CreateDoctorAccount.
//    That SP creates the User + Doctor in one transaction. There is no longer
//    a separate sp_CreateDoctor call needed here.
//
// 2. GetAllDoctorsAsync + GetDoctorByIdAsync + GetDoctorByUserIdAsync use EF Core
//    LINQ with .Include() — these are simple entity reads with navigation
//    properties. No SP needed; EF generates clean JOINs automatically.
//
// 3. UpdateDoctorAsync calls sp_UpdateDoctor via ExecuteSqlRawAsync — the SP
//    handles the IsAvailable cascade cancel logic internally.
//
// 4. GetDoctorAppointmentsAsync calls sp_GetDoctorAppointments via
//    ExecuteSqlRawAsync + SqlDataReader. The SP returns a custom projection
//    (PatientName, Status, Prescription fields) that does NOT match the
//    Appointment entity — so FromSqlRaw cannot be used here.
//
// 5. GetAvailableSlotsAsync calls sp_GetDoctorAvailableSlots which returns
//    TWO result sets: (1) schedule window, (2) booked appointment times.
//    EF Core cannot handle multiple result sets from a single SP call.
//    SqlDataReader with NextResultAsync() is used to read both sets.
//
// 6. SetScheduleAsync calls sp_SetDoctorSchedule. StartTime and EndTime
//    are passed as SqlDbType.Time to match the SP's TIME parameter type.
// ─────────────────────────────────────────────────────────────────────────────

public class DoctorRepository : IDoctorRepository
{
    private readonly ClinicDbContext _context;

    public DoctorRepository(ClinicDbContext context)
    {
        _context = context;
    }

    // ── Get All Doctors ───────────────────────────────────────────────────────
    // Uses EF Core LINQ + .Include() — cleaner than SP for a simple filtered list.
    // sp_GetAllDoctors exists but returns the same data; LINQ is used here
    // because it avoids the FromSqlRaw entity-mapping constraints.
    public async Task<List<DoctorResponseDto>> GetAllDoctorsAsync(
        bool? isAvailable = null, int? specializationId = null)
    {
        var doctors = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .AsNoTracking()
            .Where(d => d.User.IsActive
                && (isAvailable      == null || d.IsAvailable      == isAvailable)
                && (specializationId == null || d.SpecializationId == specializationId))
            .OrderBy(d => d.User.FullName)
            .ToListAsync();

        return doctors.Select(MapToDto).ToList();
    }

    // ── Get Doctor By ID ──────────────────────────────────────────────────────
    public async Task<DoctorResponseDto?> GetDoctorByIdAsync(int doctorId)
    {
        var d = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId && d.User.IsActive);

        return d == null ? null : MapToDto(d);
    }

    // ── Get Doctor By User ID ─────────────────────────────────────────────────
    public async Task<DoctorResponseDto?> GetDoctorByUserIdAsync(int userId)
    {
        var d = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.UserId == userId);

        return d == null ? null : MapToDto(d);
    }

    // ── Update Doctor ─────────────────────────────────────────────────────────
    // sp_UpdateDoctor handles: fee validation, IsAvailable cascade cancel.
    // Returns the message from the SP (includes what was cancelled if any).
    public async Task<string> UpdateDoctorAsync(int doctorId, UpdateDoctorDto dto)
    {
        var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 200)
            { Direction = ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_UpdateDoctor @DoctorId, @SpecializationId, @YearsOfExperience, " +
            "@ConsultationFee, @IsAvailable, @Message OUTPUT",
            new SqlParameter("@DoctorId",          doctorId),
            new SqlParameter("@SpecializationId",  dto.SpecializationId),
            new SqlParameter("@YearsOfExperience", dto.YearsOfExperience),
            new SqlParameter("@ConsultationFee",   dto.ConsultationFee),
            new SqlParameter("@IsAvailable",       dto.IsAvailable),
            messageParam
        );

        return (string)messageParam.Value;
    }

    // ── Get Doctor Appointments ───────────────────────────────────────────────
    // Uses sp_GetDoctorAppointments via SqlDataReader.
    // Reason: the SP returns a custom projection (PatientName from JOIN,
    // Diagnosis/Medications from Prescriptions LEFT JOIN) that does NOT map
    // to the Appointment entity — FromSqlRaw would throw a column mismatch.
    public async Task<List<AppointmentResponseDto>> GetDoctorAppointmentsAsync(
        int doctorId,
        DateOnly? fromDate  = null,
        DateOnly? toDate    = null,
        int?      statusId  = null)
    {
        var conn = (SqlConnection)_context.Database.GetDbConnection();
        await EnsureOpenAsync(conn);

        using var cmd = new SqlCommand("sp_GetDoctorAppointments", conn)
        {
            CommandType = CommandType.StoredProcedure
        };

        cmd.Parameters.AddWithValue("@DoctorId", doctorId);
        cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value =
            fromDate.HasValue ? (object)fromDate.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value;
        cmd.Parameters.Add("@ToDate",   SqlDbType.Date).Value =
            toDate.HasValue   ? (object)toDate.Value.ToDateTime(TimeOnly.MinValue)   : DBNull.Value;
        cmd.Parameters.Add("@StatusId", SqlDbType.Int).Value =
            statusId.HasValue ? (object)statusId.Value : DBNull.Value;

        var results = new List<AppointmentResponseDto>();

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new AppointmentResponseDto
            {
                AppointmentId   = reader.GetInt32(reader.GetOrdinal("AppointmentId")),
                AppointmentDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("AppointmentDate"))),
                AppointmentTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("AppointmentTime"))),
                PatientId       = reader.GetInt32(reader.GetOrdinal("PatientId")),
                PatientName     = reader.GetString(reader.GetOrdinal("PatientName")),
                PatientPhone    = reader.IsDBNull(reader.GetOrdinal("PatientPhone"))
                                    ? null : reader.GetString(reader.GetOrdinal("PatientPhone")),
                DoctorId        = doctorId,
                Status          = reader.GetString(reader.GetOrdinal("Status")),
                ReasonForVisit  = reader.IsDBNull(reader.GetOrdinal("ReasonForVisit"))
                                    ? null : reader.GetString(reader.GetOrdinal("ReasonForVisit")),
                Notes           = reader.IsDBNull(reader.GetOrdinal("Notes"))
                                    ? null : reader.GetString(reader.GetOrdinal("Notes")),
                CancellationReason = reader.IsDBNull(reader.GetOrdinal("CancellationReason"))
                                    ? null : reader.GetString(reader.GetOrdinal("CancellationReason")),

                // Prescription — only populated if LEFT JOIN returned data
                Prescription    = reader.IsDBNull(reader.GetOrdinal("Diagnosis"))
                    ? null
                    : new PrescriptionDto
                    {
                        Diagnosis   = reader.IsDBNull(reader.GetOrdinal("Diagnosis"))
                                        ? null : reader.GetString(reader.GetOrdinal("Diagnosis")),
                        Medications = reader.IsDBNull(reader.GetOrdinal("Medications"))
                                        ? null : reader.GetString(reader.GetOrdinal("Medications"))
                    }
            });
        }

        return results;
    }

    // ── Get Available Slots ───────────────────────────────────────────────────
    // sp_GetDoctorAvailableSlots returns TWO result sets:
    //   Result Set 1: schedule row (StartTime, EndTime, SlotDurationMinutes)
    //                 — empty if doctor has no schedule on this day
    //   Result Set 2: booked AppointmentTime values for the requested date
    //
    // EF Core cannot handle multiple result sets from a single SP.
    // SqlDataReader + NextResultAsync() reads both sets separately.
    // Slot calculation (start→end, step by duration, minus booked) runs in C#.
    public async Task<AvailableSlotsResponseDto> GetAvailableSlotsAsync(int doctorId, DateOnly date)
    {
        var response = new AvailableSlotsResponseDto { Date = date };

        var conn = (SqlConnection)_context.Database.GetDbConnection();
        await EnsureOpenAsync(conn);

        using var cmd = new SqlCommand("sp_GetDoctorAvailableSlots", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@DoctorId", doctorId);
        cmd.Parameters.Add("@Date", SqlDbType.Date).Value =
            date.ToDateTime(TimeOnly.MinValue);

        using var reader = await cmd.ExecuteReaderAsync();

        // ── Result Set 1: schedule window ─────────────────────────────────────
        TimeOnly? scheduleStart    = null;
        TimeOnly? scheduleEnd      = null;
        int       slotDuration     = 30;

        if (await reader.ReadAsync())
        {
            // SP returned a schedule row — doctor works on this day
            scheduleStart  = TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("StartTime")));
            scheduleEnd    = TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("EndTime")));
            slotDuration   = reader.GetInt32(reader.GetOrdinal("SlotDurationMinutes"));
        }
        // If no row in result set 1 — doctor has no schedule on this day, return empty

        // ── Result Set 2: already booked appointment times ────────────────────
        var bookedTimes = new HashSet<TimeOnly>();

        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            bookedTimes.Add(
                TimeOnly.FromTimeSpan(reader.GetTimeSpan(reader.GetOrdinal("AppointmentTime")))
            );
        }

        if (scheduleStart == null || scheduleEnd == null)
            return response; // no schedule for this day — empty slots

        // ── Compute free slots ────────────────────────────────────────────────
        response.BookedSlots = bookedTimes.Select(t => t.ToString("HH:mm")).ToList();

        var current = scheduleStart.Value;
        while (current.AddMinutes(slotDuration) <= scheduleEnd.Value)
        {
            if (!bookedTimes.Contains(current))
                response.AvailableSlots.Add(current.ToString("HH:mm"));
            current = current.AddMinutes(slotDuration);
        }

        return response;
    }

    // ── Set Schedule ──────────────────────────────────────────────────────────
    // sp_SetDoctorSchedule validates: DayOfWeek 0–6, start < end,
    // positive duration, window fits at least one slot.
    // StartTime and EndTime passed as SqlDbType.Time — matches SP TIME parameter.
    public async Task<string> SetScheduleAsync(int doctorId, DoctorScheduleDto dto)
    {
        var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 200)
            { Direction = ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_SetDoctorSchedule @DoctorId, @DayOfWeek, @StartTime, @EndTime, " +
            "@SlotDurationMinutes, @Message OUTPUT",
            new SqlParameter("@DoctorId",            doctorId),
            new SqlParameter("@DayOfWeek",           dto.DayOfWeek),
            new SqlParameter("@StartTime",  SqlDbType.Time) { Value = TimeSpan.Parse(dto.StartTime) },
            new SqlParameter("@EndTime",    SqlDbType.Time) { Value = TimeSpan.Parse(dto.EndTime) },
            new SqlParameter("@SlotDurationMinutes", dto.SlotDurationMinutes),
            messageParam
        );

        return (string)messageParam.Value;
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    // Maps Doctor entity (with User + Specialization loaded) to DoctorResponseDto
    private static DoctorResponseDto MapToDto(Models.Doctor d) => new()
    {
        DoctorId          = d.DoctorId,
        UserId            = d.UserId,
        FullName          = d.User.FullName,
        Email             = d.User.Email,
        Phone             = d.User.Phone,
        SpecializationId  = d.SpecializationId,
        Specialization    = d.Specialization.Name,
        LicenseNumber     = d.LicenseNumber,
        YearsOfExperience = d.YearsOfExperience,
        ConsultationFee   = d.ConsultationFee,
        IsAvailable       = d.IsAvailable
    };

    // Ensures the DbConnection is open before using SqlDataReader directly.
    // EF Core manages connection lifetime but it may be closed between calls.
    private static async Task EnsureOpenAsync(SqlConnection conn)
    {
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync();
    }
}
