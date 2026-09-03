IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE TABLE [LeaveTypes] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [DefaultAnnualDays] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_LeaveTypes] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_LeaveTypes_DefaultAnnualDays] CHECK ([DefaultAnnualDays] >= 0)
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [NormalizedEmail] nvarchar(256) NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE TABLE [RevokedTokens] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [JwtId] nvarchar(100) NOT NULL,
        [ExpiresAtUtc] datetime2 NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_RevokedTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RevokedTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE TABLE [Departments] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NULL,
        [ManagerEmployeeId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE TABLE [Employees] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [EmployeeNumber] nvarchar(30) NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [DepartmentId] uniqueidentifier NOT NULL,
        [ManagerId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Employees] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Employees_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Employees_Employees_ManagerId] FOREIGN KEY ([ManagerId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Employees_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE TABLE [LeaveBalances] (
        [Id] uniqueidentifier NOT NULL,
        [EmployeeId] uniqueidentifier NOT NULL,
        [LeaveTypeId] uniqueidentifier NOT NULL,
        [Year] int NOT NULL,
        [AllocatedDays] int NOT NULL,
        [UsedDays] int NOT NULL,
        [RemainingDays] int NOT NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_LeaveBalances] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_LeaveBalances_AllocatedDays] CHECK ([AllocatedDays] >= 0),
        CONSTRAINT [CK_LeaveBalances_RemainingDays] CHECK ([RemainingDays] >= 0),
        CONSTRAINT [CK_LeaveBalances_TotalDays] CHECK ([AllocatedDays] = [UsedDays] + [RemainingDays]),
        CONSTRAINT [CK_LeaveBalances_UsedDays] CHECK ([UsedDays] >= 0),
        CONSTRAINT [CK_LeaveBalances_Year] CHECK ([Year] >= 2000 AND [Year] <= 2100),
        CONSTRAINT [FK_LeaveBalances_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LeaveBalances_LeaveTypes_LeaveTypeId] FOREIGN KEY ([LeaveTypeId]) REFERENCES [LeaveTypes] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE TABLE [LeaveRequests] (
        [Id] uniqueidentifier NOT NULL,
        [EmployeeId] uniqueidentifier NOT NULL,
        [LeaveTypeId] uniqueidentifier NOT NULL,
        [StartDate] date NOT NULL,
        [EndDate] date NOT NULL,
        [WorkingDays] int NOT NULL,
        [Reason] nvarchar(1000) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [RejectionReason] nvarchar(1000) NULL,
        [ReviewedByEmployeeId] uniqueidentifier NULL,
        [ReviewedAtUtc] datetime2 NULL,
        [CancelledAtUtc] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_LeaveRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_LeaveRequests_DateRange] CHECK ([EndDate] >= [StartDate]),
        CONSTRAINT [CK_LeaveRequests_RejectionReason] CHECK ([Status] <> 'Rejected' OR LEN(LTRIM(RTRIM([RejectionReason]))) > 0),
        CONSTRAINT [CK_LeaveRequests_WorkingDays] CHECK ([WorkingDays] > 0),
        CONSTRAINT [FK_LeaveRequests_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LeaveRequests_Employees_ReviewedByEmployeeId] FOREIGN KEY ([ReviewedByEmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_LeaveRequests_LeaveTypes_LeaveTypeId] FOREIGN KEY ([LeaveTypeId]) REFERENCES [LeaveTypes] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE TABLE [IdempotencyRecords] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [RequestHash] nvarchar(64) NOT NULL,
        [LeaveRequestId] uniqueidentifier NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_IdempotencyRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_IdempotencyRecords_LeaveRequests_LeaveRequestId] FOREIGN KEY ([LeaveRequestId]) REFERENCES [LeaveRequests] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_IdempotencyRecords_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Departments_ManagerEmployeeId] ON [Departments] ([ManagerEmployeeId]) WHERE [ManagerEmployeeId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Departments_Name] ON [Departments] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_DepartmentId] ON [Employees] ([DepartmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Employees_EmployeeNumber] ON [Employees] ([EmployeeNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Employees_ManagerId] ON [Employees] ([ManagerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Employees_UserId] ON [Employees] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_IdempotencyRecords_LeaveRequestId] ON [IdempotencyRecords] ([LeaveRequestId]) WHERE [LeaveRequestId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_IdempotencyRecords_UserId_Key] ON [IdempotencyRecords] ([UserId], [Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LeaveBalances_EmployeeId_LeaveTypeId_Year] ON [LeaveBalances] ([EmployeeId], [LeaveTypeId], [Year]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LeaveBalances_LeaveTypeId] ON [LeaveBalances] ([LeaveTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LeaveRequests_EmployeeId_StartDate_EndDate] ON [LeaveRequests] ([EmployeeId], [StartDate], [EndDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LeaveRequests_EmployeeId_Status] ON [LeaveRequests] ([EmployeeId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LeaveRequests_LeaveTypeId] ON [LeaveRequests] ([LeaveTypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LeaveRequests_ReviewedByEmployeeId] ON [LeaveRequests] ([ReviewedByEmployeeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LeaveTypes_Name] ON [LeaveTypes] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RevokedTokens_ExpiresAtUtc] ON [RevokedTokens] ([ExpiresAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RevokedTokens_JwtId] ON [RevokedTokens] ([JwtId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RevokedTokens_UserId] ON [RevokedTokens] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_NormalizedEmail] ON [Users] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    ALTER TABLE [Departments] ADD CONSTRAINT [FK_Departments_Employees_ManagerEmployeeId] FOREIGN KEY ([ManagerEmployeeId]) REFERENCES [Employees] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260903082127_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260903082127_InitialCreate', N'8.0.24');
END;
GO

COMMIT;
GO

