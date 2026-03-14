using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace ClinicManagementAPI.Models
{
    public class Doctor
{
    [Key]
    public int DoctorId { get; set; }
    public int UserId { get; set; }
    public int SpecializationId { get; set; }
    [Required, MaxLength(100)]
    public string LicenseNumber { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    [Column(TypeName = "decimal(10,2)")]
    public decimal ConsultationFee { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
    public Specialization Specialization { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
}
}