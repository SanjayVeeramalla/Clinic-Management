using System.ComponentModel.DataAnnotations;

namespace ClinicManagementAPI.DTOs.Appointment;

public class CancelAppointmentDto
{
    [Required, MaxLength(500)]
    public string CancellationReason { get; set; } = string.Empty;
}