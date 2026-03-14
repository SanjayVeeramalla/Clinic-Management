using System.ComponentModel.DataAnnotations;

namespace ClinicManagementAPI.DTOs.Doctor;

public class CreateDoctorAccountDto
{
    // ── User account fields ──────────────────────────────────
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;
 
    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;
 
    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = string.Empty;
 
    [MaxLength(15), RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid Indian mobile number")]
    public string? Phone { get; set; }
 
    // ── Doctor profile fields ─────────────────────────────────
    [Required]
    public int SpecializationId { get; set; }
 
    [Required, MaxLength(100)]
    public string LicenseNumber { get; set; } = string.Empty;
 
    [Range(0, 60)]
    public int YearsOfExperience { get; set; }
 
    [Range(0, 100000)]
    public decimal ConsultationFee { get; set; }
}

public class UpdateDoctorDto
{
    [Required]
    public int SpecializationId { get; set; }

    [Range(0, 60)]
    public int YearsOfExperience { get; set; }

    [Range(0, 100000)]
    public decimal ConsultationFee { get; set; }

    public bool IsAvailable { get; set; }
}

public class DoctorResponseDto
{
    public int DoctorId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int SpecializationId { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public decimal ConsultationFee { get; set; }
    public bool IsAvailable { get; set; }
}

public class DoctorScheduleDto
{
    [Required, Range(0, 6)]
    public int DayOfWeek { get; set; }

    [Required]
    public string StartTime { get; set; } = string.Empty;

    [Required]
    public string EndTime { get; set; } = string.Empty;

    [Range(15, 120)]
    public int SlotDurationMinutes { get; set; } = 30;
}

public class AvailableSlotsResponseDto
{
    public DateOnly Date { get; set; }
    public List<string> AvailableSlots { get; set; } = new();
    public List<string> BookedSlots { get; set; } = new();
}

public class UpdateAppointmentStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class AddPrescriptionDto
{
    [Required]
    public string Diagnosis { get; set; } = string.Empty;

    [Required]
    public string Medications { get; set; } = string.Empty;

    public string? Instructions { get; set; }
    public DateOnly? FollowUpDate { get; set; }
}
