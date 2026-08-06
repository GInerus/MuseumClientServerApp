-- Создать базу данных
CREATE DATABASE MuseumDB;
GO

-- =========================
-- Используем базу
-- =========================
USE MuseumDB;
GO

-- =========================
-- Таблица: Sessions
-- =========================
CREATE TABLE Sessions
(
    SessionId INT IDENTITY(1,1) PRIMARY KEY,
    Token NVARCHAR(100) NOT NULL,
    UserType NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    LastAccess DATETIME2 NOT NULL
);
GO

-- =========================
-- Таблица: Departments (Отделы)
-- =========================
CREATE TABLE Departments
(
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ImagePath NVARCHAR(500) NULL
);
GO

-- =========================
-- Таблица: Exhibits (Экспонаты)
-- =========================
CREATE TABLE Exhibits
(
    ExhibitId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Materials NVARCHAR(300) NULL,
    IsPermanent BIT NOT NULL DEFAULT 1,
    ImagePath NVARCHAR(500) NULL,
    DepartmentId INT NOT NULL,

    CONSTRAINT FK_Exhibits_Departments
        FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId)
        ON DELETE CASCADE
);
GO

-- =========================
-- Таблица: Documents (Документы)
-- =========================
CREATE TABLE Documents
(
    DocumentId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    FileType NVARCHAR(20) NOT NULL,
    ExhibitId INT NULL,
    DepartmentId INT NULL,

    CONSTRAINT FK_Documents_Exhibits
        FOREIGN KEY (ExhibitId)
        REFERENCES Exhibits(ExhibitId)
        ON DELETE SET NULL,

    CONSTRAINT FK_Documents_Departments
        FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId)
        ON DELETE NO ACTION,

    CONSTRAINT CHK_Documents_FileType
        CHECK (FileType IN ('pdf', 'doc', 'docx', 'txt', 'md', 'ppt', 'pptx'))
);
GO

-- =========================
-- Таблица: MediaFiles (Медиа)
-- =========================
CREATE TABLE MediaFiles
(
    MediaFileId INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    MediaType NVARCHAR(20) NOT NULL,
    Description NVARCHAR(300) NULL,
    DepartmentId INT NOT NULL,

    CONSTRAINT FK_Media_Departments
        FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId)
        ON DELETE CASCADE,

    CONSTRAINT CHK_Media_Type
        CHECK (MediaType IN ('image', 'video'))
);
GO

-- =========================
-- Таблица: MuseumInfo (информация о музее, одна строка)
-- =========================
CREATE TABLE MuseumInfo
(
    MuseumInfoId INT IDENTITY(1,1) PRIMARY KEY,
    Description NVARCHAR(MAX) NULL,
    AdminPasswordHash NVARCHAR(500) NOT NULL,

    BackupFullDayOfWeek INT NOT NULL DEFAULT(0),
    BackupFullTime TIME NOT NULL DEFAULT('03:00:00'),
    BackupDifferentialTime TIME NOT NULL DEFAULT('03:00:00'),
    BackupRetentionDays INT NOT NULL DEFAULT(30)
);
GO

-- Начальная строка — переносим сюда текущий текст и пароль по умолчанию
INSERT INTO MuseumInfo 
(
    Description, 
    AdminPasswordHash, 
    BackupFullDayOfWeek, 
    BackupFullTime, 
    BackupDifferentialTime, 
    BackupRetentionDays
) 
VALUES 
(
N'Описание музея', 
N'$2a$11$ScyZQGCeneP09uny0pVNCuewMJLP.7axZej92UpqdBTiCiVOeH.5u', 
0, 
'03:00:00', 
'03:00:00', 
30
);
GO

-- =========================
-- Таблица: Logs (Логи)
-- =========================
CREATE TABLE Logs
(
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    UserType NVARCHAR(20) NOT NULL,
    Action NVARCHAR(50) NOT NULL,
    EntityType NVARCHAR(50) NULL,
    EntityName NVARCHAR(300) NULL,
    Timestamp DATETIME2 NOT NULL
);
GO

-- =========================
-- Индексы (ускорение запросов)
-- =========================
CREATE INDEX IX_Exhibits_DepartmentId
ON Exhibits(DepartmentId);

CREATE INDEX IX_Documents_DepartmentId
ON Documents(DepartmentId);

CREATE INDEX IX_Documents_ExhibitId
ON Documents(ExhibitId);

CREATE INDEX IX_MediaFiles_DepartmentId
ON MediaFiles(DepartmentId);

CREATE INDEX IX_Logs_Timestamp 
ON Logs(Timestamp DESC);

GO