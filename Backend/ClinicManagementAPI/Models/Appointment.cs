using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace ClinicManagementAPI.Models
{
    public class Appointment
{
    [Key]
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public int StatusId { get; set; }
    [MaxLength(500)]
    public string? ReasonForVisit { get; set; }
    public string? Notes { get; set; }
    [MaxLength(500)]
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public AppointmentStatus Status { get; set; } = null!;
    public Prescription? Prescription { get; set; }
}
 
}