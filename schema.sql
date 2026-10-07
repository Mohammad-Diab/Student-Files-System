-- Student Files System: the [students] database the API expects.
-- Written in 2026 from the queries in "studant api\studant api\Controllers\ValuesController.cs";
-- the 2018 project had no script. The column names bairthday and upladed_date are spelled as the code uses them.
-- Run: sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i schema.sql
-- Remove: sqlcmd -S "(localdb)\MSSQLLocalDB" -E -Q "DROP DATABASE [students]"

CREATE DATABASE [students];
GO
USE [students];
GO
CREATE TABLE dbo.students (
    Id          INT IDENTITY(1,1) PRIMARY KEY,
    first_name  NVARCHAR(100) NOT NULL,
    last_name   NVARCHAR(100) NOT NULL,
    Number      NVARCHAR(50)  NOT NULL,
    bairthday   DATETIME      NOT NULL,
    email       NVARCHAR(200) NULL,
    phone       NVARCHAR(50)  NULL,
    address     NVARCHAR(400) NULL
);
CREATE TABLE dbo.files (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    student_id   INT           NOT NULL,
    file_path    NVARCHAR(400) NOT NULL,
    title        NVARCHAR(200) NOT NULL,
    upladed_date DATETIME      NOT NULL,
    description  NVARCHAR(400) NULL
);
GO
-- Made-up demo students (4 rows = 2 pages of 2)
INSERT INTO dbo.students (first_name, last_name, Number, bairthday, email, phone, address) VALUES
 (N'Demo',   N'Student One',   N'1001', '20000115', N'demo1@example.com', N'000-0001', N'1 Test Street'),
 (N'Demo',   N'Student Two',   N'1002', '20010220', N'demo2@example.com', N'000-0002', N'2 Test Street'),
 (N'Sample', N'Student Three', N'1003', '19990325', N'demo3@example.com', N'000-0003', N'3 Test Street'),
 (N'Sample', N'Student Four',  N'1004', '20020430', N'demo4@example.com', N'000-0004', N'4 Test Street');
GO
