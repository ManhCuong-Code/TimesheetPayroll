-- ============================================================================
-- MIGRATION SCRIPT: CẬP NHẬT UTF-8 CHUẨN, LƯU ẢNH XÁC MINH & SỬA LOGIC CHẤM CÔNG
-- ============================================================================
USE [BangChamCongDB];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Thêm cột photo_in_url và photo_out_url vào bảng timesheets nếu chưa có
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.timesheets') AND name = 'photo_in_url')
BEGIN
    ALTER TABLE dbo.timesheets ADD photo_in_url NVARCHAR(500) NULL;
    PRINT N'Đã thêm cột photo_in_url vào bảng timesheets.';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.timesheets') AND name = 'photo_out_url')
BEGIN
    ALTER TABLE dbo.timesheets ADD photo_out_url NVARCHAR(500) NULL;
    PRINT N'Đã thêm cột photo_out_url vào bảng timesheets.';
END
GO

-- 2. Tạo bảng attendance_photos lưu trữ lịch sử ảnh chụp xác minh
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'attendance_photos')
BEGIN
    CREATE TABLE dbo.attendance_photos (
        id BIGINT IDENTITY(1,1) PRIMARY KEY,
        timesheet_id BIGINT NULL,
        employee_id BIGINT NOT NULL,
        punch_type NVARCHAR(20) NOT NULL, -- 'CHECK_IN' hoặc 'CHECK_OUT'
        photo_url NVARCHAR(500) NOT NULL,
        file_size_bytes INT NOT NULL DEFAULT 0,
        device_info NVARCHAR(255) NULL,
        captured_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT fk_att_photos_timesheet FOREIGN KEY (timesheet_id) REFERENCES dbo.timesheets(id) ON DELETE NO ACTION,
        CONSTRAINT fk_att_photos_employee FOREIGN KEY (employee_id) REFERENCES dbo.employees(id) ON DELETE NO ACTION
    );
    CREATE INDEX idx_att_photos_emp_date ON dbo.attendance_photos (employee_id, captured_at);
    PRINT N'Đã tạo bảng attendance_photos thành công.';
END
GO

-- 3. Cập nhật Stored Procedure sp_ResolveAttendanceShift với tiếng Việt UTF-8 chuẩn 100%
CREATE OR ALTER PROCEDURE dbo.sp_ResolveAttendanceShift
    @EmployeeId BIGINT,
    @PunchDateTime DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @PunchDateTime IS NULL
        SET @PunchDateTime = SYSDATETIME();

    DECLARE @PunchDate DATE = CAST(@PunchDateTime AS DATE);
    DECLARE @PunchTime TIME = CAST(@PunchDateTime AS TIME);
    DECLARE @Yesterday DATE = DATEADD(DAY, -1, @PunchDate);

    -- Bảng tạm chứa các ca được phân công cho nhân viên
    DECLARE @Candidates TABLE (
        AssignmentId BIGINT,
        ShiftId BIGINT,
        ShiftCode NVARCHAR(50),
        ShiftName NVARCHAR(100),
        WorkDate DATE,
        StartTime TIME,
        EndTime TIME,
        IsOvernight BIT,
        WindowStart DATETIME2,
        WindowEnd DATETIME2,
        ShiftStartDT DATETIME2,
        ShiftEndDT DATETIME2,
        CandidatePunchAction NVARCHAR(20),
        Score INT
    );

    INSERT INTO @Candidates
    SELECT 
        sa.id AS AssignmentId,
        ws.id AS ShiftId,
        ws.shift_code AS ShiftCode,
        ws.shift_name AS ShiftName,
        sa.work_date AS WorkDate,
        ws.start_time AS StartTime,
        ws.end_time AS EndTime,
        ws.is_overnight AS IsOvernight,
        -- Cửa sổ bắt đầu chấm công vào
        DATEADD(MINUTE, -ws.early_checkin_minutes, 
            DATETIME2FROMPARTS(YEAR(sa.work_date), MONTH(sa.work_date), DAY(sa.work_date), DATEPART(HOUR, ws.start_time), DATEPART(MINUTE, ws.start_time), 0, 0, 0)) AS WindowStart,
        -- Cửa sổ kết thúc chấm công ra
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

    -- 1. Kiểm tra nếu không có ca nào được phân công
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

    -- Lọc danh sách các ca ứng viên khả dụng
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
            N'Bạn đã hoàn thành lượt vào và ra cho tất cả các ca được phân công trong ngày.' AS Message,
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

-- 4. Cập nhật Stored Procedure sp_ProcessFaceAttendance với tiếng Việt UTF-8 chuẩn và lưu ảnh
CREATE OR ALTER PROCEDURE dbo.sp_ProcessFaceAttendance
    @EmployeeId BIGINT,
    @ShiftId BIGINT,
    @WorkDate DATE,
    @PunchDateTime DATETIME2 = NULL,
    @PunchAction NVARCHAR(20) = 'AUTO', -- 'AUTO', 'CHECK_IN', 'CHECK_OUT'
    @DeviceInfo NVARCHAR(255) = N'Trạm Camera Điểm Danh Văn Phòng',
    @PerformedBy NVARCHAR(100) = 'SYSTEM_KIOSK',
    @Confidence DECIMAL(5,2) = 95.0,
    @Distance DECIMAL(6,4) = 0.25,
    @PhotoUrl NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @PunchDateTime IS NULL
        SET @PunchDateTime = SYSDATETIME();

    DECLARE @PunchTime TIME = CAST(@PunchDateTime AS TIME);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Kiểm tra nhân viên có tồn tại và đang hoạt động
        DECLARE @EmpCode NVARCHAR(50);
        DECLARE @EmpName NVARCHAR(100);
        SELECT @EmpCode = employee_code, @EmpName = full_name 
        FROM dbo.employees 
        WHERE id = @EmployeeId AND status = 'ACTIVE';

        IF @EmpCode IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 
                CAST(0 AS BIT) AS Success,
                'EMPLOYEE_INACTIVE' AS ResultCode,
                N'Nhân viên không tồn tại hoặc tài khoản đã ngừng hoạt động.' AS Message,
                NULL AS PunchType, NULL AS PunchTime, NULL AS WorkShiftStatus, 0.0 AS WorkUnits, 0.0 AS ActualHours, NULL AS ShiftName;
            RETURN;
        END

        -- 2. Lấy thông tin ca làm việc
        DECLARE @ShiftCode NVARCHAR(50);
        DECLARE @ShiftName NVARCHAR(100);
        DECLARE @StartTime TIME;
        DECLARE @EndTime TIME;
        DECLARE @GracePeriodMinutes INT;
        DECLARE @ShiftUnits DECIMAL(3,2);
        DECLARE @TargetHours DECIMAL(4,2);
        DECLARE @LunchBreakHours DECIMAL(3,2);
        DECLARE @IsNightShift BIT;
        DECLARE @IsOvernight BIT;

        SELECT 
            @ShiftCode = shift_code,
            @ShiftName = shift_name,
            @StartTime = start_time,
            @EndTime = end_time,
            @GracePeriodMinutes = grace_period_minutes,
            @ShiftUnits = work_units,
            @TargetHours = target_hours,
            @LunchBreakHours = lunch_break_hours,
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

        -- 4. Xác định thao tác thực tế: CHECK_IN hay CHECK_OUT
        DECLARE @ActualAction NVARCHAR(20);
        IF @PunchAction = 'CHECK_IN'
        BEGIN
            IF @CurCheckIn IS NOT NULL
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 
                    CAST(0 AS BIT) AS Success,
                    'ALREADY_CHECKED_IN' AS ResultCode,
                    CONCAT(N'Bạn đã chấm công vào cho ca [', @ShiftName, N'] lúc ', CONVERT(VARCHAR(8), @CurCheckIn), N'. Không thể chấm vào trùng lặp.') AS Message,
                    'CHECK_IN' AS PunchType, @CurCheckIn AS PunchTime, @CurStatus AS WorkShiftStatus, 0.0 AS WorkUnits, 0.0 AS ActualHours, @ShiftName AS ShiftName;
                RETURN;
            END
            SET @ActualAction = 'CHECK_IN';
        END
        ELSE IF @PunchAction = 'CHECK_OUT'
        BEGIN
            IF @CurCheckOut IS NOT NULL
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 
                    CAST(0 AS BIT) AS Success,
                    'ALREADY_COMPLETED' AS ResultCode,
                    N'Bạn đã hoàn thành lượt vào và ra cho tất cả các ca được phân công trong ngày.' AS Message,
                    'CHECK_OUT' AS PunchType, @CurCheckOut AS PunchTime, @CurStatus AS WorkShiftStatus, 0.0 AS WorkUnits, 0.0 AS ActualHours, @ShiftName AS ShiftName;
                RETURN;
            END
            SET @ActualAction = 'CHECK_OUT';
        END
        ELSE -- AUTO
        BEGIN
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
                    N'Bạn đã hoàn thành lượt vào và ra cho tất cả các ca được phân công trong ngày.' AS Message,
                    NULL AS PunchType, @CurCheckOut AS PunchTime, @CurStatus AS WorkShiftStatus, 0.0 AS WorkUnits, 0.0 AS ActualHours, @ShiftName AS ShiftName;
                RETURN;
            END
        END

        -- 5. Xử lý ghi nhận CHECK_IN
        DECLARE @FinalStatus NVARCHAR(50);
        DECLARE @FinalWorkUnits DECIMAL(3,2) = @ShiftUnits;
        DECLARE @ActualHours DECIMAL(4,2) = 0.0;
        DECLARE @AuditReason NVARCHAR(500);

        IF @ActualAction = 'CHECK_IN'
        BEGIN
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
                    photo_in_url = COALESCE(@PhotoUrl, photo_in_url),
                    note = @AuditReason,
                    updated_at = GETDATE()
                WHERE id = @TimesheetId;
            END
            ELSE
            BEGIN
                INSERT INTO dbo.timesheets (
                    employee_id, shift_id, shift_code, shift_name, work_date,
                    check_in_time, check_out_time, actual_hours_worked, work_shift_status,
                    work_units, is_locked, attendance_source, is_confirmed, photo_in_url, note, created_at, updated_at
                )
                VALUES (
                    @EmployeeId, @ShiftId, @ShiftCode, @ShiftName, @WorkDate,
                    @PunchTime, NULL, 0.0, @FinalStatus,
                    @FinalWorkUnits, 0, 'FACE_RECOGNITION', 1, @PhotoUrl, @AuditReason, GETDATE(), GETDATE()
                );
                SET @TimesheetId = SCOPE_IDENTITY();
            END
        END
        ELSE -- 6. Xử lý ghi nhận CHECK_OUT
        BEGIN
            IF @CurCheckIn IS NULL
            BEGIN
                -- Nếu quên check-in, gán giờ bắt đầu ca làm mốc mặc định
                SET @CurCheckIn = @StartTime;
            END

            DECLARE @RawHours DECIMAL(5,2);
            IF @IsOvernight = 1 OR @EndTime < @StartTime
            BEGIN
                SET @RawHours = (24.0 - DATEPART(HOUR, @CurCheckIn) - (DATEPART(MINUTE, @CurCheckIn) / 60.0))
                               + (DATEPART(HOUR, @PunchTime) + (DATEPART(MINUTE, @PunchTime) / 60.0));
            END
            ELSE
            BEGIN
                SET @RawHours = DATEDIFF(MINUTE, @CurCheckIn, @PunchTime) / 60.0;
            END

            IF @RawHours > 5.0 AND @LunchBreakHours > 0
                SET @ActualHours = @RawHours - @LunchBreakHours;
            ELSE
                SET @ActualHours = @RawHours;

            IF @ActualHours < 0 SET @ActualHours = 0;
            IF @ActualHours > 24 SET @ActualHours = 24;

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
                photo_out_url = COALESCE(@PhotoUrl, photo_out_url),
                note = @AuditReason,
                updated_at = GETDATE()
            WHERE id = @TimesheetId;
        END

        -- 7. Lưu vào bảng lịch sử ảnh attendance_photos nếu có ảnh
        IF @PhotoUrl IS NOT NULL
        BEGIN
            INSERT INTO dbo.attendance_photos (
                timesheet_id, employee_id, punch_type, photo_url, device_info, captured_at
            )
            VALUES (
                @TimesheetId, @EmployeeId, @ActualAction, @PhotoUrl, @DeviceInfo, @PunchDateTime
            );
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
PRINT N'Đã hoàn tất cập nhật các Stored Procedures với tiếng Việt UTF-8 chuẩn!';
GO
