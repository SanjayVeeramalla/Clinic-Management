using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace ClinicManagementAPI.Models
{
    public class AuditLog
{
    [Key]
    public int LogId { get; set; }
    public int? UserId { get; set; }
    [Required, MaxLength(200)]
    public string Action { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? TableName { get; set; }
    public int? RecordId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    [MaxLength(50)]
    public string? IPAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
}