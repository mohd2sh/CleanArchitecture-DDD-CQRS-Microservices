PRINT 'Starting CMMS database initialization...';
GO


DECLARE @Asset1Id UNIQUEIDENTIFIER = '2915bd5e-b291-47ec-9e2c-0e08f2cfa2ee'; 
DECLARE @Asset2Id UNIQUEIDENTIFIER = 'a1b2c3d4-e5f6-4789-a012-b3c4d5e6f789';

DECLARE @Technician1Id UNIQUEIDENTIFIER = 'f99d735d-06f6-4541-aa46-058e8cae3b9a';
DECLARE @Technician2Id UNIQUEIDENTIFIER = 'a2b3c4d5-e6f7-4890-b123-c4d5e6f78901';
DECLARE @Technician3Id UNIQUEIDENTIFIER = 'b3c4d5e6-f7a8-4901-c234-d5e6f7890123';

DECLARE @WorkOrder1Id UNIQUEIDENTIFIER = 'e480abe7-8e0e-41c4-820b-98195d97fb12';
DECLARE @WorkOrder2Id UNIQUEIDENTIFIER = 'c84f0000-16e2-902e-2cdd-08de2a3d6c18';

DECLARE @MaintenanceRecord1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @MaintenanceRecord2Id UNIQUEIDENTIFIER = NEWID();

DECLARE @Assignment1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Assignment2Id UNIQUEIDENTIFIER = NEWID();

DECLARE @Certification1Id UNIQUEIDENTIFIER = NEWID();

GO


IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AssetsDb')
BEGIN
    CREATE DATABASE AssetsDb;
    PRINT 'Database AssetsDb created';
END
ELSE
BEGIN
    PRINT 'Database AssetsDb already exists';
END
GO

USE AssetsDb;
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'assets')
BEGIN
    EXEC('CREATE SCHEMA assets');
    PRINT 'Schema assets created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Assets' AND schema_id = SCHEMA_ID('assets'))
BEGIN
    CREATE TABLE [assets].[Assets] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(200) NOT NULL,
        [Type] NVARCHAR(100) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [Tag] NVARCHAR(100) NOT NULL,
        [Site] NVARCHAR(100) NOT NULL,
        [Area] NVARCHAR(100) NOT NULL,
        [Zone] NVARCHAR(100) NOT NULL,
        [RowVersion] ROWVERSION NOT NULL,
        [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [LastModifiedOn] DATETIME2 NULL,
        [LastModifiedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX IX_Assets_CreatedOn ON [assets].[Assets]([CreatedOn]);
    PRINT 'Table assets.Assets created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MaintenanceRecords' AND schema_id = SCHEMA_ID('assets'))
BEGIN
    CREATE TABLE [assets].[MaintenanceRecords] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [AssetId] UNIQUEIDENTIFIER NOT NULL,
        [WorkOrderId] UNIQUEIDENTIFIER NOT NULL,
        [StartedOn] DATETIME2 NOT NULL,
        [Description] NVARCHAR(500) NOT NULL,
        [PerformedBy] NVARCHAR(150) NOT NULL,
        [IsCompleted] BIT NOT NULL DEFAULT 0
    );
    CREATE INDEX IX_MaintenanceRecords_AssetId ON [assets].[MaintenanceRecords]([AssetId]);
    PRINT 'Table assets.MaintenanceRecords created';
END
GO

IF NOT EXISTS (SELECT 1 FROM [assets].[Assets])
BEGIN
    INSERT INTO [assets].[Assets] ([Id], [Name], [Type], [Status], [Tag], [Site], [Area], [Zone], [CreatedOn])
    VALUES 
        ('2915bd5e-b291-47ec-9e2c-0e08f2cfa2ee', 'Boiler Pump', 'Mechanical', 'UnderMaintenance', 'ASSET-1001', 'Plant-A', 'Floor-1', 'Zone-3', GETUTCDATE()),
        ('a1b2c3d4-e5f6-4789-a012-b3c4d5e6f789', 'HVAC Unit', 'Electrical', 'UnderMaintenance', 'ASSET-2002', 'Plant-B', 'Roof', 'Zone-1', GETUTCDATE());
    
    PRINT '2 assets inserted';
END
ELSE
BEGIN
    PRINT 'Assets seed data already exists';
END
GO

USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TechniciansDb')
BEGIN
    CREATE DATABASE TechniciansDb;
    PRINT 'Database TechniciansDb created';
END
ELSE
BEGIN
    PRINT 'Database TechniciansDb already exists';
END
GO

USE TechniciansDb;
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'technicians')
BEGIN
    EXEC('CREATE SCHEMA technicians');
    PRINT 'Schema technicians created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Technicians' AND schema_id = SCHEMA_ID('technicians'))
BEGIN
    CREATE TABLE [technicians].[Technicians] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(150) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [MaxConcurrentAssignments] INT NOT NULL,
        [SkillLevelName] NVARCHAR(50) NOT NULL,
        [SkillRank] INT NOT NULL,
        [RowVersion] ROWVERSION NOT NULL,
        [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [LastModifiedOn] DATETIME2 NULL,
        [LastModifiedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX IX_Technicians_CreatedOn ON [technicians].[Technicians]([CreatedOn]);
    PRINT 'Table technicians.Technicians created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TechnicianCertifications' AND schema_id = SCHEMA_ID('technicians'))
BEGIN
    CREATE TABLE [technicians].[TechnicianCertifications] (
        [TechnicianId] UNIQUEIDENTIFIER NOT NULL,
        [CertificationCode] NVARCHAR(50) NOT NULL,
        [IssuedOn] DATETIME2 NOT NULL,
        [ExpiresOn] DATETIME2 NULL,
        PRIMARY KEY ([TechnicianId], [CertificationCode])
    );
    CREATE INDEX IX_TechnicianCertifications_TechnicianId ON [technicians].[TechnicianCertifications]([TechnicianId]);
    PRINT 'Table technicians.TechnicianCertifications created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TechnicianAssignments' AND schema_id = SCHEMA_ID('technicians'))
BEGIN
    CREATE TABLE [technicians].[TechnicianAssignments] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TechnicianId] UNIQUEIDENTIFIER NOT NULL,
        [WorkOrderId] UNIQUEIDENTIFIER NOT NULL,
        [AssignedOn] DATETIME2 NOT NULL,
        [CompletedOn] DATETIME2 NULL
    );
    CREATE INDEX IX_TechnicianAssignments_TechnicianId ON [technicians].[TechnicianAssignments]([TechnicianId]);
    PRINT 'Table technicians.TechnicianAssignments created';
END
GO

IF NOT EXISTS (SELECT 1 FROM [technicians].[Technicians])
BEGIN
    INSERT INTO [technicians].[Technicians] ([Id], [Name], [Status], [MaxConcurrentAssignments], [SkillLevelName], [SkillRank], [CreatedOn])
    VALUES 
        ('f99d735d-06f6-4541-aa46-058e8cae3b9a', 'John Doe', 'Available', 5, 'Journeyman', 2, GETUTCDATE()),
        ('a2b3c4d5-e6f7-4890-b123-c4d5e6f78901', 'Jane Smith', 'Available', 5, 'Master', 3, GETUTCDATE()),
        ('b3c4d5e6-f7a8-4901-c234-d5e6f7890123', 'Tom Wilson', 'Available', 5, 'Apprentice', 1, GETUTCDATE());
    
    PRINT '3 technicians inserted';
    
    -- Insert certification for John Doe
    INSERT INTO [technicians].[TechnicianCertifications] ([TechnicianId], [CertificationCode], [IssuedOn], [ExpiresOn])
    VALUES 
        ('f99d735d-06f6-4541-aa46-058e8cae3b9a', 'CERT-001', GETUTCDATE(), DATEADD(DAY, 365, GETUTCDATE()));
    
    PRINT '1 certification inserted';
END
ELSE
BEGIN
    PRINT 'Technicians seed data already exists';
END
GO

USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'WorkOrdersDb')
BEGIN
    CREATE DATABASE WorkOrdersDb;
    PRINT 'Database WorkOrdersDb created';
END
ELSE
BEGIN
    PRINT 'Database WorkOrdersDb already exists';
END
GO

USE WorkOrdersDb;
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'workorders')
BEGIN
    EXEC('CREATE SCHEMA workorders');
    PRINT 'Schema workorders created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WorkOrders' AND schema_id = SCHEMA_ID('workorders'))
BEGIN
    CREATE TABLE [workorders].[WorkOrders] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Title] NVARCHAR(200) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL,
        [AssetId] UNIQUEIDENTIFIER NOT NULL,
        [TechnicianId] UNIQUEIDENTIFIER NULL,
        [Building] NVARCHAR(100) NOT NULL,
        [Floor] NVARCHAR(100) NOT NULL,
        [Room] NVARCHAR(100) NOT NULL,
        [RowVersion] ROWVERSION NOT NULL,
        [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] NVARCHAR(100) NULL,
        [LastModifiedOn] DATETIME2 NULL,
        [LastModifiedBy] NVARCHAR(100) NULL
    );
    CREATE INDEX IX_WorkOrders_CreatedOn ON [workorders].[WorkOrders]([CreatedOn]);
    PRINT 'Table workorders.WorkOrders created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WorkOrderSteps' AND schema_id = SCHEMA_ID('workorders'))
BEGIN
    CREATE TABLE [workorders].[WorkOrderSteps] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [WorkOrderId] UNIQUEIDENTIFIER NOT NULL,
        [Description] NVARCHAR(500) NOT NULL,
        [Completed] BIT NOT NULL DEFAULT 0
    );
    CREATE INDEX IX_WorkOrderSteps_WorkOrderId ON [workorders].[WorkOrderSteps]([WorkOrderId]);
    PRINT 'Table workorders.WorkOrderSteps created';
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WorkOrderComments' AND schema_id = SCHEMA_ID('workorders'))
BEGIN
    CREATE TABLE [workorders].[WorkOrderComments] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [WorkOrderId] UNIQUEIDENTIFIER NOT NULL,
        [Text] NVARCHAR(500) NOT NULL,
        [AuthorId] UNIQUEIDENTIFIER NOT NULL,
        [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE INDEX IX_WorkOrderComments_WorkOrderId ON [workorders].[WorkOrderComments]([WorkOrderId]);
    PRINT 'Table workorders.WorkOrderComments created';
END
GO

IF NOT EXISTS (SELECT 1 FROM [workorders].[WorkOrders])
BEGIN
    INSERT INTO [workorders].[WorkOrders] ([Id], [Title], [Status], [AssetId], [TechnicianId], [Building], [Floor], [Room], [CreatedOn])
    VALUES 
        ('e480abe7-8e0e-41c4-820b-98195d97fb12', 'Replace filter on HVAC Unit', 'Open', 'a1b2c3d4-e5f6-4789-a012-b3c4d5e6f789', NULL, 'Plant-B', 'Roof', 'Zone-1', GETUTCDATE()),
        ('c84f0000-16e2-902e-2cdd-08de2a3d6c18', 'Inspect boiler pump', 'Open', '2915bd5e-b291-47ec-9e2c-0e08f2cfa2ee', NULL, 'Plant-A', 'Floor-1', 'Zone-3', GETUTCDATE());
    
    PRINT '2 work orders inserted';
END
ELSE
BEGIN
    PRINT 'WorkOrders seed data already exists';
END
GO

USE AssetsDb;
GO

IF NOT EXISTS (SELECT 1 FROM [assets].[MaintenanceRecords])
BEGIN
    INSERT INTO [assets].[MaintenanceRecords] ([Id], [AssetId], [WorkOrderId], [StartedOn], [Description], [PerformedBy], [IsCompleted])
    VALUES 
        (NEWID(), '2915bd5e-b291-47ec-9e2c-0e08f2cfa2ee', 'c84f0000-16e2-902e-2cdd-08de2a3d6c18', GETUTCDATE(), 'Inspect boiler pump', 'System', 0),
        (NEWID(), 'a1b2c3d4-e5f6-4789-a012-b3c4d5e6f789', 'e480abe7-8e0e-41c4-820b-98195d97fb12', GETUTCDATE(), 'Replace filter on HVAC Unit', 'System', 0);
    
    PRINT '2 maintenance records inserted';
END
ELSE
BEGIN
    PRINT 'MaintenanceRecords seed data already exists';
END
GO

USE TechniciansDb;
GO

IF NOT EXISTS (SELECT 1 FROM [technicians].[TechnicianAssignments])
BEGIN
    INSERT INTO [technicians].[TechnicianAssignments] ([Id], [TechnicianId], [WorkOrderId], [AssignedOn], [CompletedOn])
    VALUES 
        (NEWID(), 'f99d735d-06f6-4541-aa46-058e8cae3b9a', 'e480abe7-8e0e-41c4-820b-98195d97fb12', GETUTCDATE(), NULL),
        (NEWID(), 'a2b3c4d5-e6f7-4890-b123-c4d5e6f78901', 'c84f0000-16e2-902e-2cdd-08de2a3d6c18', GETUTCDATE(), NULL);
    
    PRINT '2 technician assignments inserted';
END
ELSE
BEGIN
    PRINT 'TechnicianAssignments seed data already exists';
END
GO

PRINT 'CMMS Database initialization completed!';
GO

