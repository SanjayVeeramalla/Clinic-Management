using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace ClinicManagementAPI.Models
{
    public class AppointmentStatus
{
    [Key]
    public int StatusId { get; set; }
    [Required, MaxLength(50)]
    public string StatusName { get; set; } = string.Empty;
}
}