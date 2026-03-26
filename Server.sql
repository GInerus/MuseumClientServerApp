-- 1. Создать базу данных
CREATE DATABASE MuseumDB;
GO

-- 2. Использовать эту базу
USE MuseumDB;
GO

-- 3. Создать таблицу Sessions
CREATE TABLE Sessions
(
    SessionId INT IDENTITY(1,1) PRIMARY KEY,
    Token NVARCHAR(100) NOT NULL,
    UserType NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    LastAccess DATETIME2 NOT NULL
);
GO