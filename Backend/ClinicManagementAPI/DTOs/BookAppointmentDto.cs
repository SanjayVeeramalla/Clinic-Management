using System.ComponentModel.DataAnnotations;

namespace ClinicManagementAPI.DTOs.Appointment;

public class BookAppointmentDto
{
    [Required]
    public int DoctorId { get; set; }

    [Required]
    public DateOnly AppointmentDate { get; set; }

    [Required]
    public TimeOnly AppointmentTime { get; set; }

    [MaxLength(500)]
    public string? ReasonForVisit { get; set; }
}