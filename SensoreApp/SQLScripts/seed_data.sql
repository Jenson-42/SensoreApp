-- 1. Patient
INSERT INTO Users (FirstName, LastName, Email, Phone, IsActive, Discriminator)
VALUES ('John', 'Doe', 'john.doe@example.com', '07123456789', 1, 'Patient');

-- 2. Clinician
INSERT INTO Users (FirstName, LastName, Email, Phone, IsActive, Discriminator, WorkEmail, PersonalEmail)
VALUES ('Sarah', 'Smith', 'sarah.smith@example.com', '07234567890', 1, 'Clinician', 'sarah.smith@clinic.org', 'sarah.smith@gmail.com');

-- 3. Patient
INSERT INTO Users (FirstName, LastName, Email, Phone, IsActive, Discriminator, DateOfBirth)
VALUES ('Emily', 'Taylor', 'emily.taylor@example.com', '07345678901', 1, 'Patient', '1990-05-12');

-- 4. Clinician
INSERT INTO Users (FirstName, LastName, Email, Phone, IsActive, Discriminator, WorkEmail, PersonalEmail)
VALUES ('David', 'Nguyen', 'david.nguyen@example.com', '07456789012', 1, 'Clinician', 'd.nguyen@clinic.org', 'david.nguyen@gmail.com');

-- 5. Patient
INSERT INTO Users (FirstName, LastName, Email, Phone, IsActive, Discriminator, DateOfBirth)
VALUES ('Liam', 'Jones', 'liam.jones@example.com', '07567890123', 1, 'Patient', '1985-11-03');

-- 6. Regular User
INSERT INTO Users (FirstName, LastName, Email, Phone, IsActive, Discriminator)
VALUES ('Olivia', 'Brown', 'olivia.brown@example.com', '07678901234', 1, 'User');

-- 7. Clinician
INSERT INTO Users (FirstName, LastName, Email, Phone, IsActive, Discriminator, WorkEmail, PersonalEmail)
VALUES ('Aisha', 'Khan', 'aisha.khan@example.com', '07789012345', 1, 'Clinician', 'a.khan@clinic.org', 'aisha.khan@gmail.com');

-- 8. Patient
INSERT INTO Users (FirstName, LastName, Email, Phone, IsActive, Discriminator, DateOfBirth)
VALUES ('Noah', 'Wilson', 'noah.wilson@example.com', '07890123456', 1, 'Patient', '2000-02-28');

-- 9. Regular User
INSERT INTO Users (FirstName, LastName, Email, Phone, IsActive, Discriminator)
VALUES ('Grace', 'Lee', 'grace.lee@example.com', '07901234567', 1, 'User');

-- 10. Patient
INSERT INTO Users (FirstName, LastName, Email, Phone, IsActive, Discriminator, DateOfBirth)
VALUES ('Ethan', 'White', 'ethan.white@example.com', '07012345678', 1, 'Patient', '1995-07-19');
-------------------------------------------------------------------------------


-- Step 3: Insert new threshold settings for UserIds 1–10
INSERT INTO ThresholdSettings (UserID, ThresholdValue, CreatedAt)
VALUES (1, 80.00, GETDATE());  -- John Doe

INSERT INTO ThresholdSettings (UserID, ThresholdValue, CreatedAt)
VALUES (2, 85.00, GETDATE());  -- Sarah Smith

INSERT INTO ThresholdSettings (UserID, ThresholdValue, CreatedAt)
VALUES (3, 75.00, GETDATE());  -- Emily Taylor

INSERT INTO ThresholdSettings (UserID, ThresholdValue, CreatedAt)
VALUES (4, 90.00, GETDATE());  -- David Nguyen

INSERT INTO ThresholdSettings (UserID, ThresholdValue, CreatedAt)
VALUES (5, 70.00, GETDATE());  -- Liam Jones

INSERT INTO ThresholdSettings (UserID, ThresholdValue, CreatedAt)
VALUES (6, 88.00, GETDATE());  -- Olivia Brown

INSERT INTO ThresholdSettings (UserID, ThresholdValue, CreatedAt)
VALUES (7, 82.00, GETDATE());  -- Aisha Khan

INSERT INTO ThresholdSettings (UserID, ThresholdValue, CreatedAt)
VALUES (8, 78.00, GETDATE());  -- Noah Wilson

INSERT INTO ThresholdSettings (UserID, ThresholdValue, CreatedAt)
VALUES (9, 86.00, GETDATE());  -- Grace Lee

INSERT INTO ThresholdSettings (UserID, ThresholdValue, CreatedAt)
VALUES (10, 72.00, GETDATE()); -- Ethan White
-------------------------------------------------------------------------------

-- Step 3: Insert 10 new reports
INSERT INTO Reports (RequestedBy, DateFrom, DateTo, ComparisonDateFrom, ComparisonDateTo, UserID, FilePath, GeneratedAt, ReportType)
VALUES (2, '2025-11-01', '2025-11-07', '2025-10-01', '2025-10-07', 3, 'reports/emily_week1.pdf', GETDATE(), 'Weekly');

INSERT INTO Reports (RequestedBy, DateFrom, DateTo, ComparisonDateFrom, ComparisonDateTo, UserID, FilePath, GeneratedAt, ReportType)
VALUES (4, '2025-11-08', '2025-11-14', '2025-10-08', '2025-10-14', 5, 'reports/liam_week2.pdf', GETDATE(), 'Weekly');

INSERT INTO Reports (RequestedBy, DateFrom, DateTo, ComparisonDateFrom, ComparisonDateTo, UserID, FilePath, GeneratedAt, ReportType)
VALUES (7, '2025-11-15', '2025-11-21', NULL, NULL, 8, 'reports/noah_week3.pdf', GETDATE(), 'Weekly');

INSERT INTO Reports (RequestedBy, DateFrom, DateTo, ComparisonDateFrom, ComparisonDateTo, UserID, FilePath, GeneratedAt, ReportType)
VALUES (2, '2025-11-01', '2025-11-30', NULL, NULL, 3, 'reports/emily_monthly.pdf', GETDATE(), 'Monthly');

INSERT INTO Reports (RequestedBy, DateFrom, DateTo, ComparisonDateFrom, ComparisonDateTo, UserID, FilePath, GeneratedAt, ReportType)
VALUES (4, '2025-11-01', '2025-11-30', NULL, NULL, 5, 'reports/liam_monthly.pdf', GETDATE(), 'Monthly');

INSERT INTO Reports (RequestedBy, DateFrom, DateTo, ComparisonDateFrom, ComparisonDateTo, UserID, FilePath, GeneratedAt, ReportType)
VALUES (7, '2025-11-01', '2025-11-30', NULL, NULL, 8, 'reports/noah_monthly.pdf', GETDATE(), 'Monthly');

INSERT INTO Reports (RequestedBy, DateFrom, DateTo, ComparisonDateFrom, ComparisonDateTo, UserID, FilePath, GeneratedAt, ReportType)
VALUES (2, '2025-12-01', '2025-12-07', '2025-11-01', '2025-11-07', 3, 'reports/emily_week4.pdf', GETDATE(), 'Weekly');

INSERT INTO Reports (RequestedBy, DateFrom, DateTo, ComparisonDateFrom, ComparisonDateTo, UserID, FilePath, GeneratedAt, ReportType)
VALUES (4, '2025-12-01', '2025-12-07', '2025-11-01', '2025-11-07', 5, 'reports/liam_week4.pdf', GETDATE(), 'Weekly');

INSERT INTO Reports (RequestedBy, DateFrom, DateTo, ComparisonDateFrom, ComparisonDateTo, UserID, FilePath, GeneratedAt, ReportType)
VALUES (7, '2025-12-01', '2025-12-07', '2025-11-01', '2025-11-07', 8, 'reports/noah_week4.pdf', GETDATE(), 'Weekly');

INSERT INTO Reports (RequestedBy, DateFrom, DateTo, ComparisonDateFrom, ComparisonDateTo, UserID, FilePath, GeneratedAt, ReportType)
VALUES (2, '2025-12-01', '2025-12-31', NULL, NULL, 3, 'reports/emily_december.pdf', GETDATE(), 'Monthly');
-------------------------------------------------------------------------------

-- Report 1 (Emily Week 1)
INSERT INTO ReportMetrics (ReportID, MetricName, MetricValue, ComparisonValue)
VALUES (1, 'PeakPressureIndex', 12.34, 11.20),
       (1, 'ContactAreaPercent', 45.67, 42.50),
       (1, 'COV', 0.89, 0.85);

-- Report 2 (Liam Week 2)
INSERT INTO ReportMetrics (ReportID, MetricName, MetricValue, ComparisonValue)
VALUES (2, 'PeakPressureIndex', 14.10, 13.00),
       (2, 'ContactAreaPercent', 48.20, 46.00),
       (2, 'COV', 0.92, 0.88);

-- Report 3 (Noah Week 3)
INSERT INTO ReportMetrics (ReportID, MetricName, MetricValue, ComparisonValue)
VALUES (3, 'PeakPressureIndex', 11.75, NULL),
       (3, 'ContactAreaPercent', 43.90, NULL),
       (3, 'COV', 0.87, NULL);

-- Report 4 (Emily Monthly)
INSERT INTO ReportMetrics (ReportID, MetricName, MetricValue, ComparisonValue)
VALUES (4, 'PeakPressureIndex', 13.50, NULL),
       (4, 'ContactAreaPercent', 46.80, NULL),
       (4, 'COV', 0.91, NULL);

-- Report 5 (Liam Monthly)
INSERT INTO ReportMetrics (ReportID, MetricName, MetricValue, ComparisonValue)
VALUES (5, 'PeakPressureIndex', 15.20, NULL),
       (5, 'ContactAreaPercent', 49.10, NULL),
       (5, 'COV', 0.95, NULL);

-- Report 6 (Noah Monthly)
INSERT INTO ReportMetrics (ReportID, MetricName, MetricValue, ComparisonValue)
VALUES (6, 'PeakPressureIndex', 12.00, NULL),
       (6, 'ContactAreaPercent', 44.50, NULL),
       (6, 'COV', 0.88, NULL);

-- Report 7 (Emily Week 4)
INSERT INTO ReportMetrics (ReportID, MetricName, MetricValue, ComparisonValue)
VALUES (7, 'PeakPressureIndex', 13.80, 12.34),
       (7, 'ContactAreaPercent', 47.00, 45.67),
       (7, 'COV', 0.90, 0.89);

-- Report 8 (Liam Week 4)
INSERT INTO ReportMetrics (ReportID, MetricName, MetricValue, ComparisonValue)
VALUES (8, 'PeakPressureIndex', 15.50, 14.10),
       (8, 'ContactAreaPercent', 50.00, 48.20),
       (8, 'COV', 0.96, 0.92);

-- Report 9 (Noah Week 4)
INSERT INTO ReportMetrics (ReportID, MetricName, MetricValue, ComparisonValue)
VALUES (9, 'PeakPressureIndex', 12.20, 11.75),
       (9, 'ContactAreaPercent', 45.00, 43.90),
       (9, 'COV', 0.89, 0.87);

-- Report 10 (Emily December)
INSERT INTO ReportMetrics (ReportID, MetricName, MetricValue, ComparisonValue)
VALUES (10, 'PeakPressureIndex', 13.90, NULL),
       (10, 'ContactAreaPercent', 47.50, NULL),
       (10, 'COV', 0.92, NULL);
       -------------------------------------------------------------------------------
    
-- Report 1
INSERT INTO ReportFrames (ReportID, FrameID)
VALUES (1, 101), (1, 102), (1, 103);

-- Report 2
INSERT INTO ReportFrames (ReportID, FrameID)
VALUES (2, 104), (2, 105), (2, 106);

-- Report 3
INSERT INTO ReportFrames (ReportID, FrameID)
VALUES (3, 107), (3, 108), (3, 109);

-- Report 4
INSERT INTO ReportFrames (ReportID, FrameID)
VALUES (4, 110), (4, 111), (4, 112);

-- Report 5
INSERT INTO ReportFrames (ReportID, FrameID)
VALUES (5, 113), (5, 114), (5, 115);

-- Report 6
INSERT INTO ReportFrames (ReportID, FrameID)
VALUES (6, 116), (6, 117), (6, 118);

-- Report 7
INSERT INTO ReportFrames (ReportID, FrameID)
VALUES (7, 119), (7, 120), (7, 121);

-- Report 8
INSERT INTO ReportFrames (ReportID, FrameID)
VALUES (8, 122), (8, 123), (8, 124);

-- Report 9
INSERT INTO ReportFrames (ReportID, FrameID)
VALUES (9, 125), (9, 126), (9, 127);

-- Report 10
INSERT INTO ReportFrames (ReportID, FrameID)
VALUES (10, 128), (10, 129), (10, 130);

-------------------------------------------------------------------------------


INSERT INTO FrameMetrics (FrameID, PeakPressureIndex, ContactAreaPercent, COV, ComputedAt)
VALUES
(101, 12.34, 45.67, 0.89, GETDATE()),
(102, 12.50, 46.10, 0.88, GETDATE()),
(103, 12.20, 45.30, 0.87, GETDATE()),

(104, 14.10, 48.20, 0.92, GETDATE()),
(105, 14.30, 48.50, 0.91, GETDATE()),
(106, 14.00, 47.90, 0.90, GETDATE()),

(107, 11.75, 43.90, 0.87, GETDATE()),
(108, 11.60, 43.50, 0.86, GETDATE()),
(109, 11.80, 44.10, 0.88, GETDATE()),

(110, 13.50, 46.80, 0.91, GETDATE()),
(111, 13.70, 47.10, 0.90, GETDATE()),
(112, 13.40, 46.50, 0.89, GETDATE()),

(113, 15.20, 49.10, 0.95, GETDATE()),
(114, 15.40, 49.50, 0.94, GETDATE()),
(115, 15.10, 48.90, 0.93, GETDATE()),

(116, 12.00, 44.50, 0.88, GETDATE()),
(117, 12.20, 44.80, 0.87, GETDATE()),
(118, 11.90, 44.30, 0.86, GETDATE()),

(119, 13.80, 47.00, 0.90, GETDATE()),
(120, 13.90, 47.30, 0.89, GETDATE()),
(121, 13.70, 46.90, 0.88, GETDATE()),

(122, 15.50, 50.00, 0.96, GETDATE()),
(123, 15.60, 50.30, 0.95, GETDATE()),
(124, 15.40, 49.80, 0.94, GETDATE()),

(125, 12.20, 45.00, 0.89, GETDATE()),
(126, 12.30, 45.30, 0.88, GETDATE()),
(127, 12.10, 44.90, 0.87, GETDATE()),

(128, 13.90, 47.50, 0.92, GETDATE()),
(129, 14.00, 47.80, 0.91, GETDATE()),
(130, 13.80, 47.30, 0.90, GETDATE());
-------------------------------------------------------------------------------


INSERT INTO Alerts (UserId, TriggeringFrameId, Reason, TriggerValue, ThresholdPct, StartTime, EndTime, Status, AcknowledgedAt, CreatedAt)
VALUES
(3, 101, 'High pressure detected on left heel', 12.34, 80.00, '2025-12-01 08:00:00', '2025-12-01 08:15:00', 'New', NULL, GETDATE()),
(5, 104, 'Contact area exceeded threshold', 48.20, 70.00, '2025-12-02 09:00:00', '2025-12-02 09:20:00', 'New', NULL, GETDATE()),
(8, 107, 'COV instability detected', 0.87, 78.00, '2025-12-03 10:00:00', NULL, 'New', NULL, GETDATE()),
(3, 110, 'Peak pressure spike', 13.50, 80.00, '2025-12-04 11:00:00', '2025-12-04 11:10:00', 'Acknowledged', '2025-12-04 11:30:00', GETDATE()),
(5, 113, 'Sustained high contact area', 49.10, 70.00, '2025-12-05 12:00:00', NULL, 'New', NULL, GETDATE()),
(8, 116, 'Pressure index exceeded safe range', 12.00, 78.00, '2025-12-06 13:00:00', '2025-12-06 13:25:00', 'Acknowledged', '2025-12-06 13:45:00', GETDATE()),
(3, 119, 'COV fluctuation detected', 0.90, 80.00, '2025-12-07 14:00:00', NULL, 'New', NULL, GETDATE()),
(5, 122, 'Extreme pressure on lateral edge', 15.50, 70.00, '2025-12-08 15:00:00', '2025-12-08 15:30:00', 'New', NULL, GETDATE()),
(8, 125, 'Contact area anomaly', 45.00, 78.00, '2025-12-09 16:00:00', NULL, 'New', NULL, GETDATE()),
(3, 128, 'Pressure index trending upward', 13.90, 80.00, '2025-12-10 17:00:00', '2025-12-10 17:20:00', 'Acknowledged', '2025-12-10 17:40:00', GETDATE());
---------------------------------------------------------------------------------------------------
-- John Doe (Patient, UserId = 1) assigned to Sarah Smith (Clinician, UserId = 2)
INSERT INTO PatientClinicians (PatientID, ClinicianID)
VALUES (1, 2);

-- Emily Taylor (Patient, UserId = 3) assigned to David Nguyen (Clinician, UserId = 4)
INSERT INTO PatientClinicians (PatientID, ClinicianID)
VALUES (3, 4);

-- Liam Jones (Patient, UserId = 5) assigned to Sarah Smith (Clinician, UserId = 2)
INSERT INTO PatientClinicians (PatientID, ClinicianID)
VALUES (5, 2);

-- Noah Wilson (Patient, UserId = 8) assigned to Aisha Khan (Clinician, UserId = 7)
INSERT INTO PatientClinicians (PatientID, ClinicianID)
VALUES (8, 7);

-- Ethan White (Patient, UserId = 10) assigned to David Nguyen (Clinician, UserId = 4)
INSERT INTO PatientClinicians (PatientID, ClinicianID)
VALUES (10, 4);
