using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace ClinicManagementAPI.Models
{
    public class Prescription
{
    [Key]
    public int PrescriptionId { get; set; }
    public int AppointmentId { get; set; }
    [MaxLength(500)]
    public string? Diagnosis { get; set; }
    public string? Medications { get; set; }
    public string? Instructions { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Appointment Appointment { get; set; } = null!;
}
}