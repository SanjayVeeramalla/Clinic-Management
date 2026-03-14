using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace ClinicManagementAPI.Models
{
    public class User
{
    [Key]
    public int UserId { get; set; }
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;
    [Required, MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;
    [MaxLength(15)]
    public string? Phone { get; set; }
    public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Role Role { get; set; } = null!;
}
}