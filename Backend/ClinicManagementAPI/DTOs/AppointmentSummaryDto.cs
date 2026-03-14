
namespace ClinicManagementAPI.DTOs.Admin;

public class AppointmentSummaryDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}