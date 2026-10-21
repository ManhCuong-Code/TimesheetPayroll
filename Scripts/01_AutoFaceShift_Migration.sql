-- ============================================================================
-- HRMS PRO - MIGRATION SCRIPT: TỰ ĐỘNG XÁC ĐỊNH CA LÀM VIỆC & ĐIỂM DANH SINH TRẮC HỌC
-- CSDL: BangChamCongDB (Microsoft SQL Server)
-- ============================================================================

USE BangChamCongDB;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1. CẬP NHẬT BẢNG work_shifts: BỔ SUNG CÁC CỘT CẤU HÌNH CỬA SỔ CHẤM CÔNG VÀ CA QUA ĐÊM
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.work_shifts') AND name = 'early_checkin_minutes')
BEGIN
    ALTER TABLE dbo.work_shifts ADD early_checkin_minutes INT NOT NULL CONSTRAINT DF_work_shifts_early DEFAULT 60;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.work_shifts') AND name = 'late_checkout_minutes')
BEGIN
    ALTER TABLE dbo.work_shifts ADD late_checkout_minutes INT NOT NULL CONSTRAINT DF_work_shifts_late DEFAULT 120;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.work_shifts') AND name = 'is_overnight')
BEGIN
    ALTER TABLE dbo.work_shifts ADD is_overnight BIT NOT NULL CONSTRAINT DF_work_shifts_overnight DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.work_shifts') AND name = 'is_split_shift')
BEGIN
    ALTER TABLE dbo.work_shifts ADD is_split_shift BIT NOT NULL CONSTRAINT DF_work_shifts_split DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.work_shifts') AND name = 'split_shift_allowance')
BEGIN
    ALTER TABLE dbo.work_shifts ADD split_shift_allowance DECIMAL(15,2) NOT NULL CONSTRAINT DF_work_shifts_split_allowance DEFAULT 0.00;
END
GO

-- Cập nhật dữ liệu đặc thù cho các ca hiện tại
UPDATE dbo.work_shifts 
SET is_overnight = 1 
WHERE end_time < start_time OR is_night_shift = 1;
GO

UPDATE dbo.work_shifts 
SET is_split_shift = 1, split_shift_allowance = 50000.00 
WHERE shift_code = 'CA_GAY';
GO

-- 2. TẠO BẢNG PHÂN CÔNG CA LÀM VIỆC (shift_assignments)
IF OBJECT_ID('dbo.shift_assignments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.shift_assignments (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        employee_id BIGINT NOT NULL CONSTRAINT fk_shift_assign_emp REFERENCES dbo.employees(id) ON DELETE CASCADE,
        shift_id BIGINT NOT NULL CONSTRAINT fk_shift_assign_shift REFERENCES dbo.work_shifts(id) ON DELETE CASCADE,
        work_date DATE NOT NULL,
        status NVARCHAR(30) NOT NULL CONSTRAINT DF_shift_assign_status DEFAULT 'ACTIVE' CHECK (status IN ('ACTIVE', 'COMPLETED', 'CANCELLED')),
        assigned_by NVARCHAR(100) NULL,
        notes NVARCHAR(255) NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_shift_assign_created DEFAULT GETDATE(),
        updated_at DATETIME2 NOT NULL CONSTRAINT DF_shift_assign_updated DEFAULT GETDATE(),
        CONSTRAINT uq_emp_shift_date UNIQUE (employee_id, shift_id, work_date)
    );

    CREATE INDEX idx_shift_assign_emp_date ON dbo.shift_assignments(employee_id, work_date, status);
    CREATE INDEX idx_shift_assign_shift_date ON dbo.shift_assignments(shift_id, work_date);
END
GO

-- 3. CẬP NHẬT BẢNG timesheets: BỔ SUNG NGUỒN CHẤM CÔNG VÀ XÁC NHẬN
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.timesheets') AND name = 'attendance_source')
BEGIN
    ALTER TABLE dbo.timesheets ADD attendance_source NVARCHAR(50) NOT NULL CONSTRAINT DF_timesheets_source DEFAULT 'FACE_RECOGNITION';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.timesheets') AND name = 'is_confirmed')
BEGIN
    ALTER TABLE dbo.timesheets ADD is_confirmed BIT NOT NULL CONSTRAINT DF_timesheets_confirmed DEFAULT 1;
END
GO

-- Nâng cấp ràng buộc UNIQUE của timesheets để hỗ trợ nhân viên làm nhiều ca trong 1 ngày (ví dụ ca sáng + ca chiều)
IF EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'uq_emp_work_date' AND parent_object_id = OBJECT_ID('dbo.timesheets'))
BEGIN
    ALTER TABLE dbo.timesheets DROP CONSTRAINT uq_emp_work_date;
END
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'uq_emp_work_date_shift' AND object_id = OBJECT_ID('dbo.timesheets'))
BEGIN
    CREATE UNIQUE INDEX uq_emp_work_date_shift ON dbo.timesheets(employee_id, work_date, shift_id) WHERE shift_id IS NOT NULL;
END
GO

-- 4. TỰ ĐỘNG KHỞI TẠO LỊCH PHÂN CA MẪU CHO CÁC NHÂN VIÊN TRONG THÁNG HIỆN TẠI VÀ THÁNG TỚI
DECLARE @CurrentDate DATE = CAST(GETDATE() AS DATE);
DECLARE @StartDate DATE = DATEFROMPARTS(YEAR(@CurrentDate), MONTH(@CurrentDate), 1);
DECLARE @EndDate DATE = EOMONTH(DATEADD(MONTH, 1, @CurrentDate));

;WITH DateSeries AS (
    SELECT @StartDate AS Dt
    UNION ALL
    SELECT DATEADD(DAY, 1, Dt)
    FROM DateSeries
    WHERE Dt < @EndDate
)
INSERT INTO dbo.shift_assignments (employee_id, shift_id, work_date, status, assigned_by, notes)
SELECT 
    e.id AS employee_id,
    CASE 
        WHEN e.id = 6 AND d.Dt = @CurrentDate THEN 2 -- EMP006 làm ca sáng
        WHEN e.id = 7 THEN 5                          -- EMP007 chuyên ca đêm (22:00 - 06:00)
        WHEN e.id = 8 THEN 6                          -- EMP008 ca gãy
        ELSE 1                                        -- Mặc định Ca Hành Chính
    END AS shift_id,
    d.Dt AS work_date,
    'ACTIVE' AS status,
    N'Hệ thống tự động phân ca' AS assigned_by,
    N'Lịch phân công chuẩn' AS notes
FROM dbo.employees e
CROSS JOIN DateSeries d
WHERE DATEPART(WEEKDAY, d.Dt) <> 1 -- Nghỉ Chủ Nhật
  AND NOT EXISTS (
      SELECT 1 FROM dbo.shift_assignments sa 
      WHERE sa.employee_id = e.id 
        AND sa.work_date = d.Dt
  )
OPTION (MAXRECURSION 100);

-- Bổ sung ca thứ 2 trong ngày cho nhân viên EMP006 (Ca Chiều ID = 3) để kiểm thử trường hợp nhiều ca trong 1 ngày
IF EXISTS (SELECT 1 FROM dbo.employees WHERE id = 6)
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.shift_assignments WHERE employee_id = 6 AND shift_id = 3 AND work_date = @CurrentDate)
    BEGIN
        INSERT INTO dbo.shift_assignments (employee_id, shift_id, work_date, status, assigned_by, notes)
        VALUES (6, 3, @CurrentDate, 'ACTIVE', N'Phân công quản lý', N'Ca chiều bổ sung kiểm thử đa ca trong ngày');
    END
END
GO

-- ============================================================================
-- 5. CÁC STORED PROCEDURES CHUYÊN BIỆT
-- ============================================================================

-- ----------------------------------------------------------------------------
-- SP 1: sp_GetAssignedShiftsForAttendance
-- Lấy danh sách các ca được phân công cho nhân viên quanh thời điểm chấm công
-- ----------------------------------------------------------------------------
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAssignedShiftsForAttendance
    @EmployeeId BIGINT,
    @TargetDateTime DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @TargetDateTime IS NULL SET @TargetDateTime = GETDATE();

    DECLARE @CurrentDate DATE = CAST(@TargetDateTime AS DATE);
    DECLARE @Yesterday DATE = DATEADD(DAY, -1, @CurrentDate);
    DECLARE @Tomorrow DATE = DATEADD(DAY, 1, @CurrentDate);

    SELECT 
        sa.id AS AssignmentId,
        sa.employee_id AS EmployeeId,
        sa.work_date AS WorkDate,
        sa.status AS AssignmentStatus,
        ws.id AS ShiftId,
        ws.shift_code AS ShiftCode,
        ws.shift_name AS ShiftName,
        ws.start_time AS StartTime,
        ws.end_time AS EndTime,
        ws.lunch_break_hours AS LunchBreakHours,
        ws.target_hours AS TargetHours,
        ws.work_units AS WorkUnits,
        ws.grace_period_minutes AS GracePeriodMinutes,
        ws.early_checkin_minutes AS EarlyCheckInMinutes,
        ws.late_checkout_minutes AS LateCheckOutMinutes,
        ws.is_night_shift AS IsNightShift,
        ws.is_overnight AS IsOvernight,
        ws.is_split_shift AS IsSplitShift,
        ws.split_shift_allowance AS SplitShiftAllowance,
        -- Kiểm tra trạng thái chấm công hiện tại
        ts.id AS TimesheetId,
        ts.check_in_time AS ExistingCheckInTime,
        ts.check_out_time AS ExistingCheckOutTime,
        ts.actual_hours_worked AS ActualHoursWorked,
        ts.work_shift_status AS WorkShiftStatus
    FROM dbo.shift_assignments sa
    INNER JOIN dbo.work_shifts ws ON sa.shift_id = ws.id
    LEFT JOIN dbo.timesheets ts ON ts.employee_id = sa.employee_id 
                               AND ts.work_date = sa.work_date 
                               AND ts.shift_id = sa.shift_id
    WHERE sa.employee_id = @EmployeeId
      AND sa.status = 'ACTIVE'
      AND ws.is_active = 1
      AND (
          -- Ca trong ngày hôm nay
          sa.work_date = @CurrentDate
          -- Hoặc ca qua đêm bắt đầu từ ngày hôm qua (chưa checkout hoặc checkout sáng nay)
          OR (sa.work_date = @Yesterday AND (ws.is_overnight = 1 OR ws.end_time < ws.start_time))
      )
    ORDER BY sa.work_date ASC, ws.start_time ASC;
END
GO

-- ----------------------------------------------------------------------------
-- SP 2: sp_ResolveAttendanceShift
-- Tự động xác định ca làm việc duy nhất phù hợp từ lịch phân công và cửa sổ chấm công
-- ----------------------------------------------------------------------------
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ResolveAttendanceShift
    @EmployeeId BIGINT,
    @PunchDateTime DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @PunchDateTime IS NULL SET @PunchDateTime = GETDATE();

    DECLARE @PunchDate DATE = CAST(@PunchDateTime AS DATE);
    DECLARE @PunchTime TIME = CAST(@PunchDateTime AS TIME);
    DECLARE @Yesterday DATE = DATEADD(DAY, -1, @PunchDate);

    -- Bảng tạm chứa các ca ứng viên được phân công
    DECLARE @Candidates TABLE (
        AssignmentId BIGINT,
        WorkDate DATE,
        ShiftId BIGINT,
        ShiftCode NVARCHAR(50),
        ShiftName NVARCHAR(100),
        StartTime TIME,
        EndTime TIME,
        EarlyCheckInMinutes INT,
        LateCheckOutMinutes INT,
        GracePeriodMinutes INT,
        IsNightShift BIT,
        IsOvernight BIT,
        TimesheetId BIGINT NULL,
        CheckInTime TIME NULL,
        CheckOutTime TIME NULL,
        WorkShiftStatus NVARCHAR(50) NULL,
        WindowStart DATETIME2,
        WindowEnd DATETIME2,
        ShiftStartDT DATETIME2,
        ShiftEndDT DATETIME2,
        CandidatePunchAction NVARCHAR(20),
        Score INT -- Điểm ưu tiên xác định ca
    );

    INSERT INTO @Candidates
    SELECT 
        sa.id,
        sa.work_date,
        ws.id,
        ws.shift_code,
        ws.shift_name,
        ws.start_time,
        ws.end_time,
        ws.early_checkin_minutes,
        ws.late_checkout_minutes,
        ws.grace_period_minutes,
        ws.is_night_shift,
        ws.is_overnight,
        ts.id,
        ts.check_in_time,
        ts.check_out_time,
        ts.work_shift_status,
        -- Cửa sổ bắt đầu mở chấm công
        DATEADD(MINUTE, -ws.early_checkin_minutes, 
            DATETIME2FROMPARTS(YEAR(sa.work_date), MONTH(sa.work_date), DAY(sa.work_date), DATEPART(HOUR, ws.start_time), DATEPART(MINUTE, ws.start_time), 0, 0, 0)) AS WindowStart,
        -- Cửa sổ kết thúc chấm công
        DATEADD(MINUTE, ws.late_checkout_minutes, 
            CASE 
                WHEN ws.is_overnight = 1 OR ws.end_time < ws.start_time 
                THEN DATEADD(DAY, 1, DATETIME2FROMPARTS(YEAR(sa.work_date), MONTH(sa.work_date), DAY(sa.work_date), DATEPART(HOUR, ws.end_time), DATEPART(MINUTE, ws.end_time), 0, 0, 0))
                ELSE DATETIME2FROMPARTS(YEAR(sa.work_date), MONTH(sa.work_date), DAY(sa.work_date), DATEPART(HOUR, ws.end_time), DATEPART(MINUTE, ws.end_time), 0, 0, 0)
            END) AS WindowEnd,
        -- Thời điểm chuẩn bắt đầu ca
        DATETIME2FROMPARTS(YEAR(sa.work_date), MONTH(sa.work_date), DAY(sa.work_date), DATEPART(HOUR, ws.start_time), DATEPART(MINUTE, ws.start_time), 0, 0, 0) AS ShiftStartDT,
        -- Thời điểm chuẩn kết thúc ca
        CASE 
            WHEN ws.is_overnight = 1 OR ws.end_time < ws.start_time 
            THEN DATEADD(DAY, 1, DATETIME2FROMPARTS(YEAR(sa.work_date), MONTH(sa.work_date), DAY(sa.work_date), DATEPART(HOUR, ws.end_time), DATEPART(MINUTE, ws.end_time), 0, 0, 0))
            ELSE DATETIME2FROMPARTS(YEAR(sa.work_date), MONTH(sa.work_date), DAY(sa.work_date), DATEPART(HOUR, ws.end_time), DATEPART(MINUTE, ws.end_time), 0, 0, 0)
        END AS ShiftEndDT,
        -- Xác định loại thao tác dự kiến
        CASE 
            WHEN ts.check_in_time IS NULL THEN 'CHECK_IN'
            WHEN ts.check_out_time IS NULL THEN 'CHECK_OUT'
            ELSE 'COMPLETED'
        END AS CandidatePunchAction,
        0 AS Score
    FROM dbo.shift_assignments sa
    INNER JOIN dbo.work_shifts ws ON sa.shift_id = ws.id
    LEFT JOIN dbo.timesheets ts ON ts.employee_id = sa.employee_id 
                               AND ts.work_date = sa.work_date 
                               AND ts.shift_id = sa.shift_id
    WHERE sa.employee_id = @EmployeeId
      AND sa.status = 'ACTIVE'
      AND ws.is_active = 1
      AND (
          sa.work_date = @PunchDate
          OR (sa.work_date = @Yesterday AND (ws.is_overnight = 1 OR ws.end_time < ws.start_time))
      );

    -- 1. Kiểm tra nếu không có bất kỳ ca nào được phân công
    IF NOT EXISTS (SELECT 1 FROM @Candidates)
    BEGIN
        SELECT 
            'NO_ASSIGNED_SHIFT' AS ResolutionStatus,
            N'Bạn chưa được phân công ca làm việc phù hợp cho ngày hôm nay. Vui lòng liên hệ quản lý.' AS Message,
            NULL AS ShiftId, NULL AS ShiftCode, NULL AS ShiftName, NULL AS WorkDate, NULL AS PunchAction,
            NULL AS StartTime, NULL AS EndTime, NULL AS IsOvernight, 0 AS CandidateCount;
        RETURN;
    END

    -- 2. Đánh giá điểm ưu tiên và lọc các ca nằm trong khung giờ hợp lệ
    UPDATE @Candidates
    SET Score = 
        CASE 
            -- Ưu tiên 1: Đang có ca mở cần Check-Out và thời điểm hiện tại hợp lý
            WHEN CandidatePunchAction = 'CHECK_OUT' AND @PunchDateTime >= DATEADD(MINUTE, -120, ShiftStartDT) THEN 100
            -- Ưu tiên 2: Chưa Check-In và thời điểm nằm trong cửa sổ cho phép Check-In
            WHEN CandidatePunchAction = 'CHECK_IN' AND @PunchDateTime BETWEEN WindowStart AND WindowEnd THEN 50
            -- Ca đã hoàn thành
            WHEN CandidatePunchAction = 'COMPLETED' THEN -100
            ELSE 0
        END;

    -- Lọc danh sách các ca ứng viên khả dụng (loại bỏ các ca đã xong hoặc điểm <= 0)
    DECLARE @Eligible TABLE (
        ShiftId BIGINT,
        ShiftCode NVARCHAR(50),
        ShiftName NVARCHAR(100),
        WorkDate DATE,
        StartTime TIME,
        EndTime TIME,
        IsOvernight BIT,
        PunchAction NVARCHAR(20),
        Score INT,
        DistanceMinutes INT
    );

    INSERT INTO @Eligible
    SELECT 
        ShiftId, ShiftCode, ShiftName, WorkDate, StartTime, EndTime, IsOvernight, CandidatePunchAction, Score,
        CASE 
            WHEN CandidatePunchAction = 'CHECK_IN' THEN ABS(DATEDIFF(MINUTE, @PunchDateTime, ShiftStartDT))
            ELSE ABS(DATEDIFF(MINUTE, @PunchDateTime, ShiftEndDT))
        END AS DistanceMinutes
    FROM @Candidates
    WHERE Score > 0;

    DECLARE @MatchCount INT = (SELECT COUNT(*) FROM @Eligible);

    -- Trường hợp 1: Có đúng 1 ca phù hợp duy nhất
    IF @MatchCount = 1
    BEGIN
        SELECT TOP 1
            'RESOLVED' AS ResolutionStatus,
            N'Đã xác định ca làm việc thành công.' AS Message,
            ShiftId, ShiftCode, ShiftName, WorkDate, PunchAction, StartTime, EndTime, IsOvernight, 1 AS CandidateCount
        FROM @Eligible;
        RETURN;
    END

    -- Trường hợp 2: Có nhiều ca cùng thỏa mãn
    IF @MatchCount > 1
    BEGIN
        -- Xem xét nếu có 1 ca có Score vượt trội (ví dụ ca đang cần Check-Out)
        DECLARE @MaxScore INT = (SELECT MAX(Score) FROM @Eligible);
        DECLARE @HighScoreCount INT = (SELECT COUNT(*) FROM @Eligible WHERE Score = @MaxScore);

        IF @HighScoreCount = 1
        BEGIN
            SELECT TOP 1
                'RESOLVED' AS ResolutionStatus,
                N'Đã tự động chọn ca đang mở có độ ưu tiên cao nhất.' AS Message,
                ShiftId, ShiftCode, ShiftName, WorkDate, PunchAction, StartTime, EndTime, IsOvernight, 1 AS CandidateCount
            FROM @Eligible
            WHERE Score = @MaxScore;
            RETURN;
        END

        -- Nếu nhiều ca cùng điểm và không thể phân định một cách an toàn
        SELECT 
            'AMBIGUOUS' AS ResolutionStatus,
            N'Không xác định được ca làm việc duy nhất. Vui lòng liên hệ quản lý.' AS Message,
            NULL AS ShiftId, NULL AS ShiftCode, NULL AS ShiftName, NULL AS WorkDate, NULL AS PunchAction,
            NULL AS StartTime, NULL AS EndTime, NULL AS IsOvernight, @MatchCount AS CandidateCount;
        RETURN;
    END

    -- Trường hợp 3: Không có ca nào nằm trong cửa sổ thời gian hợp lệ
    -- Kiểm tra xem có phải tất cả các ca trong ngày đã hoàn tất?
    IF NOT EXISTS (SELECT 1 FROM @Candidates WHERE CandidatePunchAction <> 'COMPLETED')
    BEGIN
        SELECT 
            'SHIFT_COMPLETED' AS ResolutionStatus,
            N'Bạn đã hoàn thành đủ lượt vào và ra cho tất cả các ca được phân công trong ngày.' AS Message,
            NULL AS ShiftId, NULL AS ShiftCode, NULL AS ShiftName, NULL AS WorkDate, NULL AS PunchAction,
            NULL AS StartTime, NULL AS EndTime, NULL AS IsOvernight, 0 AS CandidateCount;
        RETURN;
    END

    -- Nằm ngoài khung giờ cho phép
    SELECT 
        'OUTSIDE_WINDOW' AS ResolutionStatus,
        N'Thời điểm chấm công hiện tại nằm ngoài khung giờ cho phép của ca được phân công.' AS Message,
        NULL AS ShiftId, NULL AS ShiftCode, NULL AS ShiftName, NULL AS WorkDate, NULL AS PunchAction,
        NULL AS StartTime, NULL AS EndTime, NULL AS IsOvernight, 0 AS CandidateCount;
END
GO

-- ----------------------------------------------------------------------------
-- SP 3: sp_ProcessFaceAttendance
-- Ghi nhận an toàn dữ liệu chấm công vào/ra với giao dịch SQL (Transaction & Locking)
-- ----------------------------------------------------------------------------
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ProcessFaceAttendance
    @EmployeeId BIGINT,
    @ShiftId BIGINT,
    @WorkDate DATE,
    @PunchDateTime DATETIME2 = NULL,
    @PunchAction NVARCHAR(20) = 'AUTO', -- 'AUTO', 'CHECK_IN', 'CHECK_OUT'
    @DeviceInfo NVARCHAR(255) = N'Trạm Camera Điểm Danh Sinh Trắc',
    @PerformedBy NVARCHAR(100) = 'SYSTEM_KIOSK',
    @Confidence DECIMAL(5,2) = 95.0,
    @Distance DECIMAL(6,4) = 0.25
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @PunchDateTime IS NULL SET @PunchDateTime = GETDATE();
    DECLARE @PunchTime TIME = CAST(@PunchDateTime AS TIME);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Kiểm tra phân công ca hợp lệ (Authorization)
        IF NOT EXISTS (
            SELECT 1 FROM dbo.shift_assignments WITH (UPDLOCK, ROWLOCK)
            WHERE employee_id = @EmployeeId AND shift_id = @ShiftId AND work_date = @WorkDate AND status = 'ACTIVE'
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 
                CAST(0 AS BIT) AS Success,
                'NO_VALID_ASSIGNMENT' AS ResultCode,
                N'Nhân viên không có lịch phân công hợp lệ cho ca này vào ngày chỉ định.' AS Message,
                NULL AS PunchType, NULL AS PunchTime, NULL AS WorkShiftStatus, 0.0 AS WorkUnits, 0.0 AS ActualHours, NULL AS ShiftName;
            RETURN;
        END

        -- 2. Đọc thông tin cấu hình ca làm việc
        DECLARE @ShiftCode NVARCHAR(50), @ShiftName NVARCHAR(100);
        DECLARE @StartTime TIME, @EndTime TIME;
        DECLARE @LunchBreakHours DECIMAL(4,2), @TargetHours DECIMAL(4,2), @ShiftUnits DECIMAL(3,2);
        DECLARE @GracePeriodMinutes INT, @IsNightShift BIT, @IsOvernight BIT;

        SELECT 
            @ShiftCode = shift_code,
            @ShiftName = shift_name,
            @StartTime = start_time,
            @EndTime = end_time,
            @LunchBreakHours = lunch_break_hours,
            @TargetHours = target_hours,
            @ShiftUnits = work_units,
            @GracePeriodMinutes = grace_period_minutes,
            @IsNightShift = is_night_shift,
            @IsOvernight = is_overnight
        FROM dbo.work_shifts
        WHERE id = @ShiftId;

        -- 3. Khóa và đọc bản ghi timesheets hiện có (Tránh Race Condition giữa 2 request đồng thời)
        DECLARE @TimesheetId BIGINT = NULL;
        DECLARE @CurCheckIn TIME = NULL;
        DECLARE @CurCheckOut TIME = NULL;
        DECLARE @CurStatus NVARCHAR(50) = NULL;
        DECLARE @IsLocked BIT = 0;

        SELECT 
            @TimesheetId = id,
            @CurCheckIn = check_in_time,
            @CurCheckOut = check_out_time,
            @CurStatus = work_shift_status,
            @IsLocked = is_locked
        FROM dbo.timesheets WITH (UPDLOCK, ROWLOCK)
        WHERE employee_id = @EmployeeId AND work_date = @WorkDate AND shift_id = @ShiftId;

        IF @IsLocked = 1
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 
                CAST(0 AS BIT) AS Success,
                'TIMESHEET_LOCKED' AS ResultCode,
                N'Bản ghi chấm công của ngày này đã được chốt/khóa sổ. Không thể sửa đổi.' AS Message,
                NULL AS PunchType, NULL AS PunchTime, NULL AS WorkShiftStatus, 0.0 AS WorkUnits, 0.0 AS ActualHours, @ShiftName AS ShiftName;
            RETURN;
        END

        -- 4. Kiểm tra Cooldown chống quét trùng lặp trong vòng 3 phút
        DECLARE @LastPunchTime TIME = COALESCE(@CurCheckOut, @CurCheckIn);
        IF @LastPunchTime IS NOT NULL
        BEGIN
            DECLARE @DiffMins INT = ABS(DATEDIFF(MINUTE, @LastPunchTime, @PunchTime));
            IF @DiffMins < 3 AND CAST(@PunchDateTime AS DATE) = @WorkDate
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 
                    CAST(0 AS BIT) AS Success,
                    'COOLDOWN_ACTIVE' AS ResultCode,
                    CONCAT(N'Thao tác quá nhanh. Bạn vừa chấm công lúc ', CONVERT(VARCHAR(8), @LastPunchTime), N'. Vui lòng đợi 3 phút.') AS Message,
                    NULL AS PunchType, @LastPunchTime AS PunchTime, @CurStatus AS WorkShiftStatus, 0.0 AS WorkUnits, 0.0 AS ActualHours, @ShiftName AS ShiftName;
                RETURN;
            END
        END

        -- 5. Xác định thao tác thực tế: CHECK_IN hay CHECK_OUT
        DECLARE @ActualAction NVARCHAR(20);
        IF @CurCheckIn IS NULL
        BEGIN
            SET @ActualAction = 'CHECK_IN';
        END
        ELSE IF @CurCheckOut IS NULL
        BEGIN
            SET @ActualAction = 'CHECK_OUT';
        END
        ELSE
        BEGIN
            -- Đã có cả lượt vào và ra => Ca đã hoàn tất!
            ROLLBACK TRANSACTION;
            SELECT 
                CAST(0 AS BIT) AS Success,
                'ALREADY_COMPLETED' AS ResultCode,
                N'Bạn đã hoàn thành đủ cả lượt vào và lượt ra cho ca làm việc này.' AS Message,
                NULL AS PunchType, @CurCheckOut AS PunchTime, @CurStatus AS WorkShiftStatus, 0.0 AS WorkUnits, 0.0 AS ActualHours, @ShiftName AS ShiftName;
            RETURN;
        END

        -- 6. Xử lý ghi nhận CHECK_IN
        DECLARE @FinalStatus NVARCHAR(50);
        DECLARE @FinalWorkUnits DECIMAL(3,2) = @ShiftUnits;
        DECLARE @ActualHours DECIMAL(4,2) = 0.0;
        DECLARE @AuditReason NVARCHAR(500);

        IF @ActualAction = 'CHECK_IN'
        BEGIN
            -- Tính toán đi muộn
            DECLARE @LateMinutes INT = DATEDIFF(MINUTE, @StartTime, @PunchTime);
            IF @LateMinutes > @GracePeriodMinutes
            BEGIN
                SET @FinalStatus = 'LATE';
                SET @AuditReason = CONCAT(N'Chấm công vào tự động (Đi muộn ', @LateMinutes, N' phút)');
            END
            ELSE
            BEGIN
                SET @FinalStatus = 'PRESENT';
                SET @AuditReason = N'Chấm công vào đúng giờ (Khuôn mặt)';
            END

            IF @TimesheetId IS NOT NULL
            BEGIN
                UPDATE dbo.timesheets
                SET check_in_time = @PunchTime,
                    work_shift_status = @FinalStatus,
                    work_units = @FinalWorkUnits,
                    attendance_source = 'FACE_RECOGNITION',
                    note = @AuditReason,
                    updated_at = GETDATE()
                WHERE id = @TimesheetId;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.timesheets (
                    employee_id, shift_id, shift_code, shift_name, work_date,
                    check_in_time, check_out_time, actual_hours_worked, work_shift_status,
                    work_units, is_locked, attendance_source, is_confirmed, note, created_at, updated_at
                )
                VALUES (
                    @EmployeeId, @ShiftId, @ShiftCode, @ShiftName, @WorkDate,
                    @PunchTime, NULL, 0.0, @FinalStatus,
                    @FinalWorkUnits, 0, 'FACE_RECOGNITION', 1, @AuditReason, GETDATE(), GETDATE()
                );
                SET @TimesheetId = SCOPE_IDENTITY();
            END
        END
        ELSE -- 7. Xử lý ghi nhận CHECK_OUT
        BEGIN
            -- Tính số giờ làm việc thực tế
            DECLARE @RawHours DECIMAL(5,2);
            IF @IsOvernight = 1 OR @EndTime < @StartTime
            BEGIN
                -- Ca qua đêm: từ CheckIn hôm trước đến PunchTime hôm nay
                SET @RawHours = (24.0 - DATEPART(HOUR, @CurCheckIn) - (DATEPART(MINUTE, @CurCheckIn) / 60.0))
                               + (DATEPART(HOUR, @PunchTime) + (DATEPART(MINUTE, @PunchTime) / 60.0));
            END
            ELSE
            BEGIN
                SET @RawHours = DATEDIFF(MINUTE, @CurCheckIn, @PunchTime) / 60.0;
            END

            IF @RawHours > 5.0 AND @LunchBreakHours > 0
            BEGIN
                SET @ActualHours = @RawHours - @LunchBreakHours;
            END
            ELSE
            BEGIN
                SET @ActualHours = @RawHours;
            END
            IF @ActualHours < 0 SET @ActualHours = 0;
            IF @ActualHours > 24 SET @ActualHours = 24;

            -- Đánh giá về sớm
            DECLARE @EarlyMinutes INT = DATEDIFF(MINUTE, @PunchTime, @EndTime);
            IF @IsOvernight = 0 AND @EarlyMinutes > @GracePeriodMinutes
            BEGIN
                SET @FinalStatus = CASE WHEN @CurStatus = 'LATE' THEN 'LATE' ELSE 'EARLY_LEAVE' END;
                SET @AuditReason = CONCAT(N'Chấm công ra ca (Về sớm ', @EarlyMinutes, N' phút)');
            END
            ELSE
            BEGIN
                SET @FinalStatus = COALESCE(@CurStatus, 'PRESENT');
                SET @AuditReason = N'Chấm công ra ca hoàn tất (Khuôn mặt)';
            END

            -- Quy đổi ngày công
            IF @ActualHours >= (@TargetHours * 0.85)
                SET @FinalWorkUnits = @ShiftUnits;
            ELSE IF @ActualHours >= (@TargetHours * 0.45)
                SET @FinalWorkUnits = ROUND(@ShiftUnits * 0.5, 2);
            ELSE
                SET @FinalWorkUnits = ROUND((@ActualHours / NULLIF(@TargetHours, 0)) * @ShiftUnits, 2);

            UPDATE dbo.timesheets
            SET check_out_time = @PunchTime,
                actual_hours_worked = @ActualHours,
                work_shift_status = @FinalStatus,
                work_units = @FinalWorkUnits,
                note = @AuditReason,
                updated_at = GETDATE()
            WHERE id = @TimesheetId;
        END

        -- 8. Ghi nhật ký kiểm toán khuôn mặt (face_audit_logs)
        INSERT INTO dbo.face_audit_logs (
            employee_id, action_type, similarity_score, performed_by, device_info, is_success, created_at
        )
        VALUES (
            @EmployeeId,
            CASE WHEN @ActualAction = 'CHECK_IN' THEN 'PUNCH_CHECK_IN' ELSE 'PUNCH_CHECK_OUT' END,
            @Confidence / 100.0,
            @PerformedBy,
            @DeviceInfo,
            1,
            GETDATE()
        );

        -- 9. Ghi nhật ký lịch sử chấm công (timesheet_audit_logs)
        INSERT INTO dbo.timesheet_audit_logs (
            timesheet_id, employee_id, action, before_data, after_data, modified_by, modified_at, reason
        )
        VALUES (
            @TimesheetId,
            @EmployeeId,
            CASE WHEN @ActualAction = 'CHECK_IN' THEN 'FACE_CHECK_IN' ELSE 'FACE_CHECK_OUT' END,
            CONCAT('{"checkIn":"', @CurCheckIn, '","checkOut":"', @CurCheckOut, '"}'),
            CONCAT('{"checkIn":"', COALESCE(@CurCheckIn, @PunchTime), '","checkOut":"', CASE WHEN @ActualAction = 'CHECK_OUT' THEN @PunchTime ELSE NULL END, '"}'),
            @PerformedBy,
            GETDATE(),
            CONCAT(@AuditReason, N' • Độ tin cậy sinh trắc: ', @Confidence, N'%')
        );

        COMMIT TRANSACTION;

        -- 10. Trả về kết quả thành công
        SELECT 
            CAST(1 AS BIT) AS Success,
            'SUCCESS' AS ResultCode,
            CONCAT(
                N'Ghi nhận ', 
                CASE WHEN @ActualAction = 'CHECK_IN' THEN N'VÀO CA (Check-In)' ELSE N'HẾT CA (Check-Out)' END,
                N' thành công cho ca [', @ShiftName, N'] lúc ',
                CONVERT(VARCHAR(8), @PunchTime)
            ) AS Message,
            @ActualAction AS PunchType,
            @PunchTime AS PunchTime,
            @FinalStatus AS WorkShiftStatus,
            @FinalWorkUnits AS WorkUnits,
            @ActualHours AS ActualHours,
            @ShiftName AS ShiftName;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
        SELECT 
            CAST(0 AS BIT) AS Success,
            'SQL_ERROR' AS ResultCode,
            CONCAT(N'Lỗi hệ thống khi ghi nhận chấm công: ', @ErrMsg) AS Message,
            NULL AS PunchType, NULL AS PunchTime, NULL AS WorkShiftStatus, 0.0 AS WorkUnits, 0.0 AS ActualHours, NULL AS ShiftName;
    END CATCH
END
GO

-- ----------------------------------------------------------------------------
-- SP 4: sp_GetAttendanceStatus
-- Truy vấn trạng thái chấm công của nhân viên theo ngày
-- ----------------------------------------------------------------------------
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAttendanceStatus
    @EmployeeId BIGINT,
    @TargetDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @TargetDate IS NULL SET @TargetDate = CAST(GETDATE() AS DATE);

    SELECT 
        sa.id AS AssignmentId,
        sa.employee_id AS EmployeeId,
        e.employee_code AS EmployeeCode,
        e.full_name AS FullName,
        sa.work_date AS WorkDate,
        ws.id AS ShiftId,
        ws.shift_code AS ShiftCode,
        ws.shift_name AS ShiftName,
        ws.start_time AS StartTime,
        ws.end_time AS EndTime,
        ws.is_night_shift AS IsNightShift,
        ws.is_overnight AS IsOvernight,
        ts.id AS TimesheetId,
        ts.check_in_time AS CheckInTime,
        ts.check_out_time AS CheckOutTime,
        ts.actual_hours_worked AS ActualHoursWorked,
        ts.work_shift_status AS WorkShiftStatus,
        ts.work_units AS WorkUnits,
        ts.attendance_source AS AttendanceSource,
        ts.is_locked AS IsLocked,
        ts.note AS Note,
        CASE 
            WHEN ts.check_in_time IS NULL THEN 'NOT_STARTED'
            WHEN ts.check_out_time IS NULL THEN 'IN_PROGRESS'
            ELSE 'COMPLETED'
        END AS StatusCategory
    FROM dbo.shift_assignments sa
    INNER JOIN dbo.employees e ON sa.employee_id = e.id
    INNER JOIN dbo.work_shifts ws ON sa.shift_id = ws.id
    LEFT JOIN dbo.timesheets ts ON ts.employee_id = sa.employee_id 
                               AND ts.work_date = sa.work_date 
                               AND ts.shift_id = sa.shift_id
    WHERE sa.employee_id = @EmployeeId
      AND sa.work_date = @TargetDate
      AND sa.status = 'ACTIVE'
    ORDER BY ws.start_time ASC;
END
GO

-- ----------------------------------------------------------------------------
-- SP 5: sp_GetMonthlyAttendanceSummary
-- Tổng hợp bảng công theo tháng phục vụ báo cáo và kế toán tính lương
-- ----------------------------------------------------------------------------
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetMonthlyAttendanceSummary
    @Month INT,
    @Year INT,
    @EmployeeId BIGINT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        e.id AS EmployeeId,
        e.employee_code AS EmployeeCode,
        e.full_name AS FullName,
        d.department_name AS DepartmentName,
        COUNT(DISTINCT sa.id) AS TotalAssignedShifts,
        COUNT(DISTINCT CASE WHEN ts.check_in_time IS NOT NULL THEN ts.id END) AS TotalPresentShifts,
        COUNT(DISTINCT CASE WHEN ts.work_shift_status = 'LATE' THEN ts.id END) AS TotalLateCount,
        COUNT(DISTINCT CASE WHEN ts.work_shift_status = 'EARLY_LEAVE' THEN ts.id END) AS TotalEarlyLeaveCount,
        ISNULL(SUM(ts.work_units), 0) AS TotalWorkUnits,
        ISNULL(SUM(ts.actual_hours_worked), 0) AS TotalActualHours,
        ISNULL(SUM(CASE WHEN ts.actual_hours_worked > 8.0 THEN ts.actual_hours_worked - 8.0 ELSE 0 END), 0) AS TotalOvertimeHours
    FROM dbo.employees e
    LEFT JOIN dbo.departments d ON e.department_id = d.id
    LEFT JOIN dbo.shift_assignments sa ON sa.employee_id = e.id 
                                      AND MONTH(sa.work_date) = @Month 
                                      AND YEAR(sa.work_date) = @Year
                                      AND sa.status = 'ACTIVE'
    LEFT JOIN dbo.timesheets ts ON ts.employee_id = sa.employee_id 
                               AND ts.work_date = sa.work_date 
                               AND ts.shift_id = sa.shift_id
    WHERE e.status = 'ACTIVE'
      AND (@EmployeeId IS NULL OR e.id = @EmployeeId)
    GROUP BY e.id, e.employee_code, e.full_name, d.department_name
    ORDER BY e.employee_code ASC;
END
GO

PRINT N'Hoàn tất cập nhật Migration tự động nhận diện ca làm việc & điểm danh sinh trắc học!';
GO
