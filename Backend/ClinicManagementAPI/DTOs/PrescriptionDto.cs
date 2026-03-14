namespace ClinicManagementAPI.DTOs.Appointment;
public class PrescriptionDto
{
    public int PrescriptionId { get; set; }
    public string? Diagnosis { get; set; }
    public string? Medications { get; set; }
    public string? Instructions { get; set; }
    public DateOnly? FollowUpDate { get; set; }
}