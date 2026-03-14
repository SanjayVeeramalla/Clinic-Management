using Microsoft.AspNetCore.Mvc;
using ClinicManagementAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecializationsController : ControllerBase
{
    private readonly ClinicDbContext _context;

    public SpecializationsController(ClinicDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var specs = await _context.Specializations.AsNoTracking()
            .Select(s => new { s.SpecializationId, s.Name, s.Description })
            .ToListAsync();
        return Ok(new { success = true, data = specs });
    }
}