using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using ClinicManagementAPI.Data;
using ClinicManagementAPI.Filters;
using ClinicManagementAPI.Helpers;
using ClinicManagementAPI.Middleware;
using ClinicManagementAPI.Repositories;
using ClinicManagementAPI.Repositories.Interfaces;
using ClinicManagementAPI.Services;
using ClinicManagementAPI.Services.Interfaces;

// ─── Serilog: configure directly in code (no ReadFrom.Configuration needed) ──
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("Logs/clinic-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Clinic Management API...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for all ASP.NET Core logging
    builder.Host.UseSerilog();

    // ─── Database ─────────────────────────────────────────────────────────────
    builder.Services.AddDbContext<ClinicDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sql => sql.CommandTimeout(60)));

    // ─── JWT Authentication ───────────────────────────────────────────────────
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(secretKey),
            ValidateIssuer           = true,
            ValidIssuer              = jwtSettings["Issuer"],
            ValidateAudience         = true,
            ValidAudience            = jwtSettings["Audience"],
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Log.Warning("JWT auth failed: {Error}", ctx.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    // ─── CORS ─────────────────────────────────────────────────────────────────
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ClinicCors", policy =>
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials());
    });

    // ─── Dependency Injection ─────────────────────────────────────────────────
    builder.Services.AddScoped<IAuthRepository,        AuthRepository>();
    builder.Services.AddScoped<IDoctorRepository,      DoctorRepository>();
    builder.Services.AddScoped<IPatientRepository,     PatientRepository>();
    builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
    builder.Services.AddScoped<IAdminRepository,       AdminRepository>();

    builder.Services.AddScoped<IAuthService,        AuthService>();
    builder.Services.AddScoped<IDoctorService,      DoctorService>();
    builder.Services.AddScoped<IPatientService,     PatientService>();
    builder.Services.AddScoped<IAppointmentService, AppointmentService>();
    builder.Services.AddScoped<IAdminService,       AdminService>();

    builder.Services.AddSingleton<JwtHelper>();

    // ─── Controllers + Validation Filter ──────────────────────────────────────
    builder.Services.AddControllers(options =>
        options.Filters.Add<ValidationFilter>());

    builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
        options.SuppressModelStateInvalidFilter = true);

    // ─── Swagger ──────────────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title       = "Clinic Management System API",
            Version     = "v1",
            Description = "Full-stack clinic management with JWT, roles, and stored procedures"
        });

        var securityScheme = new OpenApiSecurityScheme
        {
            Name        = "Authorization",
            Description = "Enter: Bearer {your JWT token}",
            In          = ParameterLocation.Header,
            Type        = SecuritySchemeType.ApiKey,
            Scheme      = "Bearer"
        };

        c.AddSecurityDefinition("Bearer", securityScheme);
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                        { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });

    // ─── Build & Configure Pipeline ───────────────────────────────────────────
    var app = builder.Build();

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clinic API v1"));
    }

    app.UseHttpsRedirection();
    app.UseCors("ClinicCors");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Clinic Management API is running. Swagger: http://localhost:5000/swagger");
    app.Run();
}
catch (Exception ex)
{
    // This catches startup crashes and prints them clearly
    Log.Fatal(ex, "Application failed to start: {Message}", ex.Message);
}
finally
{
    Log.CloseAndFlush();
}