using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace ClinicManagementAPI.Models
{
    public class DoctorSchedule
{
    [Key]
    public int ScheduleId { get; set; }
    public int DoctorId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotDurationMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public Doctor Doctor { get; set; } = null!;
}
}