-- Create Habits table to store habit definitions
IF NOT EXISTS (SELECT *
FROM sys.tables
WHERE name = 'Habits')
BEGIN
  CREATE TABLE dbo.Habits
  (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL UNIQUE,
    Unit NVARCHAR(100) NOT NULL
  );
END

-- Create HabitLogs table to store habit occurrences
IF NOT EXISTS (SELECT *
FROM sys.tables
WHERE name = 'HabitLogs')
BEGIN
  CREATE TABLE dbo.HabitLogs
  (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    HabitId INT NOT NULL,
    [Date] DATE NOT NULL,
    Quantity INT NOT NULL,
    Notes NVARCHAR(MAX),
    FOREIGN KEY (HabitId) REFERENCES dbo.Habits(Id) ON DELETE CASCADE
  );
END
