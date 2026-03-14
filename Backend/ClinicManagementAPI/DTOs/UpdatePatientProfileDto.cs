using System.ComponentModel.DataAnnotations;

namespace ClinicManagementAPI.DTOs.Patient;

public class UpdatePatientProfileDto
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(15), RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid Indian mobile number")]
    public string? Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [RegularExpression("Male|Female|Other", ErrorMessage = "Gender must be Male, Female, or Other")]
    public string? Gender { get; set; }

    [MaxLength(5)]
    public string? BloodGroup { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(15)]
    public string? EmergencyContact { get; set; }

    public string? MedicalHistory { get; set; }
}