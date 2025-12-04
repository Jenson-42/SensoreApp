-- Insert 5 test users (Patients and Clinicians)
INSERT INTO Users (FirstName, LastName, Email, Phone, IsActive)
VALUES 
    ('John', 'Doe', 'john.doe@example.com', '07700900001', 1),
    ('Jane', 'Smith', 'jane.smith@example.com', '07700900002', 1),
    ('Robert', 'Johnson', 'robert.johnson@example.com', '07700900003', 1),
    ('Emily', 'Williams', 'emily.williams@example.com', '07700900004', 1),
    ('Michael', 'Brown', 'michael.brown@example.com', '07700900005', 0);
----------------------------------------------------------------------------------------------------------------------
-- Add alerts for John Doe (UserId = 1)
INSERT INTO Alerts (UserId, TriggeringFrameId, Reason, TriggerValue, ThresholdPct, StartTime, EndTime, Status, AcknowledgedAt, CreatedAt)
VALUES 
    (1, 1, 'Peak pressure exceeded threshold', 540.00, 80.00, DATEADD(HOUR, -2, GETDATE()), DATEADD(HOUR, -1, GETDATE()), 'Resolved', DATEADD(HOUR, -1, GETDATE()), DATEADD(HOUR, -2, GETDATE())),
    (1, 2, 'Sustained high pressure detected', 420.00, 80.00, DATEADD(MINUTE, -45, GETDATE()), NULL, 'New', NULL, DATEADD(MINUTE, -45, GETDATE())),
    (1, 1, 'Pressure distribution uneven', 350.00, 80.00, DATEADD(HOUR, -5, GETDATE()), DATEADD(HOUR, -4, GETDATE()), 'Resolved', DATEADD(HOUR, -4, GETDATE()), DATEADD(HOUR, -5, GETDATE()));

-- Add alerts for Jane Smith (UserId = 2)
INSERT INTO Alerts (UserId, TriggeringFrameId, Reason, TriggerValue, ThresholdPct, StartTime, EndTime, Status, AcknowledgedAt, CreatedAt)
VALUES 
    (2, 1, 'Critical pressure level reached', 650.00, 75.00, DATEADD(HOUR, -3, GETDATE()), DATEADD(HOUR, -2, GETDATE()), 'Resolved', DATEADD(HOUR, -2, GETDATE()), DATEADD(HOUR, -3, GETDATE())),
    (2, 2, 'High pressure in lower right quadrant', 580.00, 75.00, DATEADD(MINUTE, -30, GETDATE()), NULL, 'Active', NULL, DATEADD(MINUTE, -30, GETDATE()));

-- Add alerts for Robert Johnson (UserId = 3)
INSERT INTO Alerts (UserId, TriggeringFrameId, Reason, TriggerValue, ThresholdPct, StartTime, EndTime, Status, AcknowledgedAt, CreatedAt)
VALUES 
    (3, 1, 'Moderate pressure detected', 450.00, 85.00, DATEADD(HOUR, -1, GETDATE()), NULL, 'New', NULL, DATEADD(HOUR, -1, GETDATE())),
    (3, 2, 'Contact area reduced significantly', 280.00, 85.00, DATEADD(HOUR, -6, GETDATE()), DATEADD(HOUR, -5, GETDATE()), 'Resolved', DATEADD(HOUR, -5, GETDATE()), DATEADD(HOUR, -6, GETDATE())),
    (3, 1, 'Pressure ulcer risk identified', 720.00, 85.00, DATEADD(MINUTE, -90, GETDATE()), DATEADD(MINUTE, -60, GETDATE()), 'Resolved', DATEADD(MINUTE, -60, GETDATE()), DATEADD(MINUTE, -90, GETDATE()));
----------------------------------------------------------------------------------------------------------------------------
-- Add threshold settings for different users
INSERT INTO ThresholdSettings (UserID, ThresholdValue, CreatedAt)
VALUES 
    (1, 80.00, GETDATE()),      -- John Doe: Default threshold
    (2, 65.00, GETDATE()),      -- Jane Smith: Custom lower threshold (more sensitive)
    (3, 90.00, GETDATE()),      -- Robert Johnson: Custom higher threshold (less sensitive)
    (4, 75.00, GETDATE());      -- Emily Williams: Custom threshold

-- UserID 5 (Michael Brown) has NO threshold setting - will use default 80%