using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ClinicManagementAPI.Data;
using ClinicManagementAPI.DTOs.Auth;
using ClinicManagementAPI.Repositories.Interfaces;

namespace ClinicManagementAPI.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly ClinicDbContext _context;

    public AuthRepository(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<(int UserId, string Message)> RegisterUserAsync(RegisterRequestDto dto, string passwordHash)
    {
        var userIdParam = new SqlParameter("@UserId", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
        var messageParam = new SqlParameter("@Message", System.Data.SqlDbType.NVarChar, 200) { Direction = System.Data.ParameterDirection.Output };

        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_RegisterUser @FullName, @Email, @PasswordHash, @Phone, @RoleName, @UserId OUTPUT, @Message OUTPUT",
            new SqlParameter("@FullName", dto.FullName),
            new SqlParameter("@Email", dto.Email),
            new SqlParameter("@PasswordHash", passwordHash),
            new SqlParameter("@Phone", (object?)dto.Phone ?? DBNull.Value),
            new SqlParameter("@RoleName", dto.Role),
            userIdParam,
            messageParam
        );

        return ((int)userIdParam.Value, (string)messageParam.Value);
    }

    // EF Rule: FromSqlRaw maps to the full User entity — the SP must return ALL
    // entity columns (UserId, FullName, Email, PasswordHash, Phone, RoleId,
    // IsActive, CreatedAt, UpdatedAt). Cannot include extra JOIN columns like RoleName.
    // RoleName is loaded separately via a standard LINQ query on Roles.
    public async Task<UserInfoDto?> GetUserByEmailAsync(string email)
    {
        // Step 1: load User via SP — SP now returns all User entity columns only
        var users = await _context.Users
            .FromSqlRaw("EXEC sp_GetUserByEmail @Email",
                new SqlParameter("@Email", email))
            .AsNoTracking()
            .ToListAsync();

        var user = users.FirstOrDefault();
        if (user == null) return null;

        // Step 2: load RoleName via LINQ — .Include() is forbidden on FromSqlRaw results
        var roleName = await _context.Roles
            .Where(r => r.RoleId == user.RoleId)
            .Select(r => r.RoleName)
            .FirstOrDefaultAsync();

        return new UserInfoDto
        {
            UserId   = user.UserId,
            FullName = user.FullName,
            Email    = user.Email,
            Phone    = user.Phone,
            Role     = roleName ?? string.Empty
        };
    }

    public async Task<string?> GetPasswordHashAsync(string email)
    {
        // Simple column projection — no SP needed, standard LINQ is fine
        return await _context.Users
            .Where(u => u.Email == email && u.IsActive)
            .Select(u => u.PasswordHash)
            .FirstOrDefaultAsync();
    }

    public async Task<UserInfoDto?> GetUserByIdAsync(int userId)
    {
        // Step 1: load User via SP
        var users = await _context.Users
            .FromSqlRaw("EXEC sp_GetUserById @UserId",
                new SqlParameter("@UserId", userId))
            .AsNoTracking()
            .ToListAsync();

        var user = users.FirstOrDefault();
        if (user == null) return null;

        // Step 2: load RoleName via LINQ
        var roleName = await _context.Roles
            .Where(r => r.RoleId == user.RoleId)
            .Select(r => r.RoleName)
            .FirstOrDefaultAsync();

        return new UserInfoDto
        {
            UserId   = user.UserId,
            FullName = user.FullName,
            Email    = user.Email,
            Phone    = user.Phone,
            Role     = roleName ?? string.Empty
        };
    }

    public async Task SaveRefreshTokenAsync(int userId, string token, DateTime expiresAt)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_SaveRefreshToken @UserId, @Token, @ExpiresAt",
            new SqlParameter("@UserId", userId),
            new SqlParameter("@Token", token),
            new SqlParameter("@ExpiresAt", expiresAt)
        );
    }

    // EF Rule: FromSqlRaw maps to RefreshToken entity — SP returns only RefreshToken
    // columns (TokenId, UserId, Token, ExpiresAt, IsRevoked, CreatedAt).
    // User/Role info is loaded separately via standard LINQ after the SP call.
    public async Task<(int UserId, string Email, string Role, DateTime ExpiresAt, bool IsRevoked)?>
        GetRefreshTokenAsync(string token)
    {
        // Step 1: load RefreshToken entity via SP
        var result = await _context.RefreshTokens
            .FromSqlRaw("EXEC sp_GetRefreshToken @Token",
                new SqlParameter("@Token", token))
            .AsNoTracking()
            .ToListAsync();

        var rt = result.FirstOrDefault();
        if (rt == null) return null;

        // Step 2: load User + Role via standard LINQ — .Include() on FromSqlRaw is forbidden
        var user = await _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == rt.UserId);

        if (user == null) return null;

        return (rt.UserId, user.Email, user.Role.RoleName, rt.ExpiresAt, rt.IsRevoked);
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "EXEC sp_RevokeRefreshToken @Token",
            new SqlParameter("@Token", token)
        );
    }
}