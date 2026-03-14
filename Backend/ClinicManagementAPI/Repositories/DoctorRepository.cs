using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ClinicManagementAPI.Data;
using ClinicManagementAPI.DTOs.Appointment;
using ClinicManagementAPI.DTOs.Doctor;
using ClinicManagementAPI.Repositories.Interfaces;

namespace ClinicManagementAPI.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly ClinicDbContext _context;

    public DoctorRepository(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<(int DoctorId, string Message)> CreateDoctorAsync(CreateDoctorDto dto)
    {
        var doctorIdParam = new SqlParameter("@DoctorId", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        var messageParam = new SqlParameter("@Message", System.Data.SqlDbType.NVarChar, 200) { Direction = System.Data.ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_CreateDoctor @UserId, @SpecializationId, @LicenseNumber, @YearsOfExperience, @ConsultationFee, @DoctorId OUTPUT, @Message OUTPUT",
            new SqlParameter("@UserId", dto.UserId),
            new SqlParameter("@SpecializationId", dto.SpecializationId),
            new SqlParameter("@LicenseNumber", dto.LicenseNumber),
            new SqlParameter("@YearsOfExperience", dto.YearsOfExperience),
            new SqlParameter("@ConsultationFee", dto.ConsultationFee),
            doctorIdParam,
            messageParam
        );

        return ((int)doctorIdParam.Value, (string)messageParam.Value);
    }

    public async Task<List<DoctorResponseDto>> GetAllDoctorsAsync(bool? isAvailable = null, int? specializationId = null)
    {
        var doctors = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .AsNoTracking()
            .Where(d => d.User.IsActive
                && (isAvailable == null || d.IsAvailable == isAvailable)
                && (specializationId == null || d.SpecializationId == specializationId))
            .OrderBy(d => d.User.FullName)
            .ToListAsync();

        return doctors.Select(d => new DoctorResponseDto
        {
            DoctorId = d.DoctorId,
            UserId = d.UserId,
            FullName = d.User.FullName,
            Email = d.User.Email,
            Phone = d.User.Phone,
            SpecializationId = d.SpecializationId,
            Specialization = d.Specialization.Name,
            LicenseNumber = d.LicenseNumber,
            YearsOfExperience = d.YearsOfExperience,
            ConsultationFee = d.ConsultationFee,
            IsAvailable = d.IsAvailable
        }).ToList();
    }

    public async Task<DoctorResponseDto?> GetDoctorByIdAsync(int doctorId)
    {
        var d = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

        if (d == null) return null;

        return new DoctorResponseDto
        {
            DoctorId = d.DoctorId, UserId = d.UserId, FullName = d.User.FullName,
            Email = d.User.Email, Phone = d.User.Phone, SpecializationId = d.SpecializationId,
            Specialization = d.Specialization.Name, LicenseNumber = d.LicenseNumber,
            YearsOfExperience = d.YearsOfExperience, ConsultationFee = d.ConsultationFee,
            IsAvailable = d.IsAvailable
        };
    }

    public async Task<DoctorResponseDto?> GetDoctorByUserIdAsync(int userId)
    {
        var d = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Specialization)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (d == null) return null;

        return new DoctorResponseDto
        {
            DoctorId = d.DoctorId, UserId = d.UserId, FullName = d.User.FullName,
            Email = d.User.Email, Phone = d.User.Phone, SpecializationId = d.SpecializationId,
            Specialization = d.Specialization.Name, LicenseNumber = d.LicenseNumber,
            YearsOfExperience = d.YearsOfExperience, ConsultationFee = d.ConsultationFee,
            IsAvailable = d.IsAvailable
        };
    }

    public async Task<string> UpdateDoctorAsync(int doctorId, UpdateDoctorDto dto)
    {
        var messageParam = new SqlParameter("@Message", System.Data.SqlDbType.NVarChar, 200) { Direction = System.Data.ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_UpdateDoctor @DoctorId, @SpecializationId, @YearsOfExperience, @ConsultationFee, @IsAvailable, @Message OUTPUT",
            new SqlParameter("@DoctorId", doctorId),
            new SqlParameter("@SpecializationId", dto.SpecializationId),
            new SqlParameter("@YearsOfExperience", dto.YearsOfExperience),
            new SqlParameter("@ConsultationFee", dto.ConsultationFee),
            new SqlParameter("@IsAvailable", dto.IsAvailable),
            messageParam
        );

        return (string)messageParam.Value;
    }

    public async Task<List<AppointmentResponseDto>> GetDoctorAppointmentsAsync(
        int doctorId, DateOnly? fromDate = null, DateOnly? toDate = null, int? statusId = null)
    {
        var appointments = await _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.Status)
            .Include(a => a.Prescription)
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId
                && (fromDate == null || a.AppointmentDate >= fromDate)
                && (toDate == null || a.AppointmentDate <= toDate)
                && (statusId == null || a.StatusId == statusId))
            .OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.AppointmentTime)
            .ToListAsync();

        return appointments.Select(a => new AppointmentResponseDto
        {
            AppointmentId = a.AppointmentId,
            AppointmentDate = a.AppointmentDate,
            AppointmentTime = a.AppointmentTime,
            PatientId = a.PatientId,
            PatientName = a.Patient.User.FullName,
            PatientPhone = a.Patient.User.Phone,
            DoctorId = a.DoctorId,
            DoctorName = string.Empty,
            Status = a.Status.StatusName,
            ReasonForVisit = a.ReasonForVisit,
            Notes = a.Notes
        }).ToList();
    }

    public async Task<AvailableSlotsResponseDto> GetAvailableSlotsAsync(int doctorId, DateOnly date)
    {
        var dayOfWeek = (int)date.DayOfWeek;
        var schedule = await _context.DoctorSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.DayOfWeek == dayOfWeek && s.IsActive);

        var response = new AvailableSlotsResponseDto { Date = date };

        if (schedule == null) return response;

        var bookedTimes = await _context.Appointments
            .Include(a => a.Status)
            .AsNoTracking()
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate == date
                && a.Status.StatusName != "Cancelled" && a.Status.StatusName != "NoShow")
            .Select(a => a.AppointmentTime)
            .ToListAsync();

        response.BookedSlots = bookedTimes.Select(t => t.ToString("HH:mm")).ToList();

        var current = schedule.StartTime;
        while (current.AddMinutes(schedule.SlotDurationMinutes) <= schedule.EndTime)
        {
            if (!bookedTimes.Contains(current))
                response.AvailableSlots.Add(current.ToString("HH:mm"));
            current = current.AddMinutes(schedule.SlotDurationMinutes);
        }

        return response;
    }

    public async Task<string> SetScheduleAsync(int doctorId, DoctorScheduleDto dto)
    {
        var messageParam = new SqlParameter("@Message", System.Data.SqlDbType.NVarChar, 200) { Direction = System.Data.ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_SetDoctorSchedule @DoctorId, @DayOfWeek, @StartTime, @EndTime, @SlotDurationMinutes, @Message OUTPUT",
            new SqlParameter("@DoctorId", doctorId),
            new SqlParameter("@DayOfWeek", dto.DayOfWeek),
            new SqlParameter("@StartTime", dto.StartTime),
            new SqlParameter("@EndTime", dto.EndTime),
            new SqlParameter("@SlotDurationMinutes", dto.SlotDurationMinutes),
            messageParam
        );

        return (string)messageParam.Value;
    }
}