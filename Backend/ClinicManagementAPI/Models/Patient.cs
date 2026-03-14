using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace ClinicManagementAPI.Models
{
    public class Patient
{
    [Key]
    public int PatientId { get; set; }
    public int UserId { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    [MaxLength(10)]
    public string? Gender { get; set; }
    [MaxLength(5)]
    public string? BloodGroup { get; set; }
    [MaxLength(300)]
    public string? Address { get; set; }
    [MaxLength(15)]
    public string? EmergencyContact { get; set; }
    public string? MedicalHistory { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
}