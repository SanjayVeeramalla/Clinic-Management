-- ============================================================
-- Clinic Management System - Database Creation Script
-- ============================================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'ClinicManagementDB')
BEGIN
    ALTER DATABASE ClinicManagementDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ClinicManagementDB;
END
GO

CREATE DATABASE ClinicManagementDB;
GO

USE ClinicManagementDB;
GO

-- ============================================================
-- TABLES
-- ============================================================

CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
GO

CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(150) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    Phone NVARCHAR(15),
    RoleId INT NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);
GO

CREATE TABLE RefreshTokens (
    TokenId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Token NVARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt DATETIME2 NOT NULL,
    IsRevoked BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

CREATE TABLE Specializations (
    SpecializationId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500)
);
GO

CREATE TABLE Doctors (
    DoctorId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL UNIQUE,
    SpecializationId INT NOT NULL,
    LicenseNumber NVARCHAR(100) NOT NULL UNIQUE,
    YearsOfExperience INT DEFAULT 0,
    ConsultationFee DECIMAL(10,2) NOT NULL DEFAULT 0,
    IsAvailable BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Doctors_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Doctors_Specializations FOREIGN KEY (SpecializationId) REFERENCES Specializations(SpecializationId)
);
GO

CREATE TABLE Patients (
    PatientId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL UNIQUE,
    DateOfBirth DATE,
    Gender NVARCHAR(10) CHECK (Gender IN ('Male','Female','Other')),
    BloodGroup NVARCHAR(5),
    Address NVARCHAR(300),
    EmergencyContact NVARCHAR(15),
    MedicalHistory NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Patients_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

CREATE TABLE DoctorSchedules (
    ScheduleId INT PRIMARY KEY IDENTITY(1,1),
    DoctorId INT NOT NULL,
    DayOfWeek INT NOT NULL CHECK (DayOfWeek BETWEEN 0 AND 6), -- 0=Sunday
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    SlotDurationMinutes INT NOT NULL DEFAULT 30,
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_DoctorSchedules_Doctors FOREIGN KEY (DoctorId) REFERENCES Doctors(DoctorId),
    CONSTRAINT UQ_DoctorSchedule UNIQUE (DoctorId, DayOfWeek)
);
GO

CREATE TABLE AppointmentStatuses (
    StatusId INT PRIMARY KEY IDENTITY(1,1),
    StatusName NVARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE Appointments (
    AppointmentId INT PRIMARY KEY IDENTITY(1,1),
    PatientId INT NOT NULL,
    DoctorId INT NOT NULL,
    AppointmentDate DATE NOT NULL,
    AppointmentTime TIME NOT NULL,
    StatusId INT NOT NULL,
    ReasonForVisit NVARCHAR(500),
    Notes NVARCHAR(MAX),
    CancellationReason NVARCHAR(500),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Appointments_Patients FOREIGN KEY (PatientId) REFERENCES Patients(PatientId),
    CONSTRAINT FK_Appointments_Doctors FOREIGN KEY (DoctorId) REFERENCES Doctors(DoctorId),
    CONSTRAINT FK_Appointments_Statuses FOREIGN KEY (StatusId) REFERENCES AppointmentStatuses(StatusId),
    CONSTRAINT UQ_DoctorSlot UNIQUE (DoctorId, AppointmentDate, AppointmentTime)
);
GO

CREATE TABLE Prescriptions (
    PrescriptionId INT PRIMARY KEY IDENTITY(1,1),
    AppointmentId INT NOT NULL UNIQUE,
    Diagnosis NVARCHAR(500),
    Medications NVARCHAR(MAX),
    Instructions NVARCHAR(MAX),
    FollowUpDate DATE,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Prescriptions_Appointments FOREIGN KEY (AppointmentId) REFERENCES Appointments(AppointmentId)
);
GO

CREATE TABLE AuditLogs (
    LogId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT,
    Action NVARCHAR(200) NOT NULL,
    TableName NVARCHAR(100),
    RecordId INT,
    OldValues NVARCHAR(MAX),
    NewValues NVARCHAR(MAX),
    IPAddress NVARCHAR(50),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
GO

-- ============================================================
-- INDEXES
-- ============================================================
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Appointments_PatientId ON Appointments(PatientId);
CREATE INDEX IX_Appointments_DoctorId ON Appointments(DoctorId);
CREATE INDEX IX_Appointments_Date ON Appointments(AppointmentDate);
CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
GO

-- ============================================================
-- SEED DATA
-- ============================================================
INSERT INTO Roles (RoleName) VALUES ('Admin'), ('Doctor'), ('Patient');

INSERT INTO AppointmentStatuses (StatusName) 
VALUES ('Pending'), ('Confirmed'), ('Completed'), ('Cancelled'), ('NoShow');

INSERT INTO Specializations (Name, Description) VALUES
('General Medicine', 'General health and wellness'),
('Cardiology', 'Heart and cardiovascular diseases'),
('Dermatology', 'Skin, hair and nail conditions'),
('Orthopedics', 'Bones, joints and muscles'),
('Pediatrics', 'Children healthcare'),
('Gynecology', 'Women reproductive health'),
('Neurology', 'Brain and nervous system'),
('Ophthalmology', 'Eye disorders');

-- Admin user (password: Admin@123)
INSERT INTO Users (FullName, Email, PasswordHash, Phone, RoleId)
VALUES ('System Admin', 'admin@clinic.com', 
'$2a$11$rBV2JDeWW3.vKMBHpSf/AOm3/KSQkQd.Va1G1QkFp9JHxBIumLKHi', 
'9999999999', 1);

PRINT 'Database and seed data created successfully.';
GO
