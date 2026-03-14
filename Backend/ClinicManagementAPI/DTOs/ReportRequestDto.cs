using System.ComponentModel.DataAnnotations;

namespace ClinicManagementAPI.DTOs.Admin;

public class ReportRequestDto
{
    [Required]
    public DateOnly FromDate { get; set; }

    [Required]
    public DateOnly ToDate { get; set; }
}