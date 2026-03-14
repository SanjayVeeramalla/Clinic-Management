namespace ClinicManagement.API.DTOs.Appointment;
public class CreateDoctorAccountResponseDto
{
    public int UserId { get; set; }
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
