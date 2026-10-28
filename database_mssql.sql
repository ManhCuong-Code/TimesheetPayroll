-- ============================================================================
-- HRMS PRO - TIMESHEET & PAYROLL ENTERPRISE DATABASE SYSTEM
-- CƠ SỞ DỮ LIỆU DOANH NGHIỆP: BangChamCongDB (MICROSOFT SQL SERVER)
-- HỆ THỐNG QUẢN LÝ NHÂN SỰ – CHẤM CÔNG – TÍNH LƯƠNG
-- CÔNG TY TNHH HỆ THỐNG QUẢN TRỊ DOANH NGHIỆP
-- ============================================================================

-- BƯỚC 1: TẠO VÀ SỬ DỤNG CSDL RIÊNG 'BangChamCongDB'
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BangChamCongDB')
BEGIN
    CREATE DATABASE BangChamCongDB COLLATE Vietnamese_CI_AS;
END
GO

USE BangChamCongDB;
GO

-- ============================================================================
-- BƯỚC 2: XÓA BẢNG CŨ (NẾU CÓ) THEO THỨ TỰ PHỤ THUỘC TRONG BangChamCongDB
-- ============================================================================
IF OBJECT_ID('dbo.attendance_requests', 'U') IS NOT NULL DROP TABLE dbo.attendance_requests;
IF OBJECT_ID('dbo.timesheet_audit_logs', 'U') IS NOT NULL DROP TABLE dbo.timesheet_audit_logs;
IF OBJECT_ID('dbo.face_audit_logs', 'U') IS NOT NULL DROP TABLE dbo.face_audit_logs;
IF OBJECT_ID('dbo.employee_face_profiles', 'U') IS NOT NULL DROP TABLE dbo.employee_face_profiles;
IF OBJECT_ID('dbo.payslip_lines', 'U') IS NOT NULL DROP TABLE dbo.payslip_lines;
IF OBJECT_ID('dbo.payslips', 'U') IS NOT NULL DROP TABLE dbo.payslips;
IF OBJECT_ID('dbo.leave_requests', 'U') IS NOT NULL DROP TABLE dbo.leave_requests;
IF OBJECT_ID('dbo.timesheets', 'U') IS NOT NULL DROP TABLE dbo.timesheets;
IF OBJECT_ID('dbo.shift_assignments', 'U') IS NOT NULL DROP TABLE dbo.shift_assignments;
IF OBJECT_ID('dbo.work_shifts', 'U') IS NOT NULL DROP TABLE dbo.work_shifts;
IF OBJECT_ID('dbo.payroll_periods', 'U') IS NOT NULL DROP TABLE dbo.payroll_periods;
IF OBJECT_ID('dbo.deduction_rates', 'U') IS NOT NULL DROP TABLE dbo.deduction_rates;
IF OBJECT_ID('dbo.contracts', 'U') IS NOT NULL DROP TABLE dbo.contracts;
IF OBJECT_ID('dbo.user_accounts', 'U') IS NOT NULL DROP TABLE dbo.user_accounts;
IF OBJECT_ID('dbo.employees', 'U') IS NOT NULL DROP TABLE dbo.employees;
IF OBJECT_ID('dbo.departments', 'U') IS NOT NULL DROP TABLE dbo.departments;
GO

-- ============================================================================
-- BƯỚC 3: TẠO 15 BẢNG NGHIỆP VỤ QUẢN LÝ NHÂN SỰ, CHẤM CÔNG & TÍNH LƯƠNG
-- ============================================================================

CREATE TABLE departments (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    department_code NVARCHAR(50) NOT NULL UNIQUE,
    department_name NVARCHAR(150) NOT NULL,
    manager_name NVARCHAR(100) NULL,
    description NVARCHAR(MAX) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE employees (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    employee_code NVARCHAR(30) NOT NULL UNIQUE,
    full_name NVARCHAR(100) NOT NULL,
    date_of_birth DATE NULL,
    gender NVARCHAR(10) NULL,
    identity_card_number NVARCHAR(20) NOT NULL UNIQUE,
    email NVARCHAR(120) NOT NULL UNIQUE,
    phone_number NVARCHAR(20) NULL,
    bank_account_number NVARCHAR(50) NOT NULL,
    bank_name NVARCHAR(100) NOT NULL,
    department_id BIGINT NULL CONSTRAINT fk_employees_dept REFERENCES departments(id) ON DELETE SET NULL,
    hire_date DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    status NVARCHAR(30) NOT NULL DEFAULT 'ACTIVE' CHECK (status IN ('ACTIVE', 'ON_LEAVE', 'RESIGNED')),
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE user_accounts (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    employee_id BIGINT NOT NULL UNIQUE CONSTRAINT fk_users_emp REFERENCES employees(id) ON DELETE CASCADE,
    username NVARCHAR(50) NOT NULL UNIQUE,
    password_hash NVARCHAR(255) NOT NULL,
    role NVARCHAR(50) NOT NULL DEFAULT 'ROLE_EMPLOYEE' CHECK (role IN ('ROLE_EMPLOYEE', 'ROLE_HR_ACCOUNTANT', 'ROLE_MANAGER', 'ROLE_ADMIN')),
    is_active BIT NOT NULL DEFAULT 1,
    last_login_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE contracts (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    employee_id BIGINT NOT NULL CONSTRAINT fk_contracts_emp REFERENCES employees(id) ON DELETE CASCADE,
    contract_number NVARCHAR(50) NOT NULL UNIQUE,
    contract_type NVARCHAR(50) NOT NULL DEFAULT 'FULL_TIME' CHECK (contract_type IN ('PROBATION', 'FULL_TIME', 'INDEFINITE')),
    base_salary DECIMAL(15, 2) NOT NULL CHECK (base_salary >= 0),
    meal_allowance DECIMAL(15, 2) NOT NULL DEFAULT 0.00 CHECK (meal_allowance >= 0),
    fuel_allowance DECIMAL(15, 2) NOT NULL DEFAULT 0.00 CHECK (fuel_allowance >= 0),
    start_date DATE NOT NULL,
    end_date DATE NULL,
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT chk_contract_dates CHECK (end_date IS NULL OR end_date >= start_date)
);
GO

CREATE TABLE payroll_periods (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    period_code NVARCHAR(30) NOT NULL UNIQUE,
    period_month INT NOT NULL CHECK (period_month BETWEEN 1 AND 12),
    period_year INT NOT NULL CHECK (period_year >= 2020),
    standard_work_days DECIMAL(4, 2) NOT NULL DEFAULT 22.00 CHECK (standard_work_days > 0),
    status NVARCHAR(30) NOT NULL DEFAULT 'DRAFT' CHECK (status IN ('DRAFT', 'CALCULATING', 'CALCULATED', 'LOCKED', 'PAID')),
    total_gross_amount DECIMAL(15, 2) NOT NULL DEFAULT 0.00,
    total_net_amount DECIMAL(15, 2) NOT NULL DEFAULT 0.00,
    locked_at DATETIME2 NULL,
    locked_by NVARCHAR(100) NULL,
    paid_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT uq_period_month_year UNIQUE (period_month, period_year)
);
GO

CREATE TABLE deduction_rates (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    rate_code NVARCHAR(30) NOT NULL UNIQUE,
    rate_name NVARCHAR(100) NOT NULL,
    employee_rate DECIMAL(6, 4) NOT NULL CHECK (employee_rate >= 0),
    employer_rate DECIMAL(6, 4) NOT NULL CHECK (employer_rate >= 0),
    effective_from DATE NOT NULL,
    effective_to DATE NULL,
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT chk_deduction_dates CHECK (effective_to IS NULL OR effective_to >= effective_from)
);
GO

CREATE TABLE work_shifts (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    shift_code NVARCHAR(50) NOT NULL UNIQUE,
    shift_name NVARCHAR(100) NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    lunch_break_hours DECIMAL(4, 2) NOT NULL DEFAULT 1.50,
    target_hours DECIMAL(4, 2) NOT NULL DEFAULT 8.00,
    work_units DECIMAL(3, 2) NOT NULL DEFAULT 1.00,
    grace_period_minutes INT NOT NULL DEFAULT 15,
    early_checkin_minutes INT NOT NULL DEFAULT 60,
    late_checkout_minutes INT NOT NULL DEFAULT 120,
    is_night_shift BIT NOT NULL DEFAULT 0,
    is_overnight BIT NOT NULL DEFAULT 0,
    is_split_shift BIT NOT NULL DEFAULT 0,
    split_shift_allowance DECIMAL(15, 2) NOT NULL DEFAULT 0.00,
    is_active BIT NOT NULL DEFAULT 1,
    description NVARCHAR(255) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE shift_assignments (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    employee_id BIGINT NOT NULL CONSTRAINT fk_shift_assign_emp REFERENCES employees(id) ON DELETE CASCADE,
    shift_id BIGINT NOT NULL CONSTRAINT fk_shift_assign_shift REFERENCES work_shifts(id) ON DELETE CASCADE,
    work_date DATE NOT NULL,
    status NVARCHAR(30) NOT NULL DEFAULT 'ACTIVE' CHECK (status IN ('ACTIVE', 'COMPLETED', 'CANCELLED')),
    assigned_by NVARCHAR(100) NULL,
    notes NVARCHAR(255) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT uq_emp_shift_date UNIQUE (employee_id, shift_id, work_date)
);
GO

CREATE INDEX idx_shift_assign_emp_date ON shift_assignments(employee_id, work_date, status);
CREATE INDEX idx_shift_assign_shift_date ON shift_assignments(shift_id, work_date);
GO

CREATE TABLE timesheets (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    employee_id BIGINT NOT NULL CONSTRAINT fk_timesheets_emp REFERENCES employees(id) ON DELETE CASCADE,
    period_id BIGINT NULL CONSTRAINT fk_timesheets_period REFERENCES payroll_periods(id) ON DELETE NO ACTION,
    shift_id BIGINT NULL CONSTRAINT fk_timesheets_shift REFERENCES work_shifts(id) ON DELETE SET NULL,
    shift_code NVARCHAR(50) NULL,
    shift_name NVARCHAR(100) NULL,
    work_date DATE NOT NULL,
    check_in_time TIME NULL,
    check_out_time TIME NULL,
    actual_hours_worked DECIMAL(4, 2) NOT NULL DEFAULT 0.00 CHECK (actual_hours_worked >= 0),
    work_shift_status NVARCHAR(50) NOT NULL DEFAULT 'PRESENT' CHECK (work_shift_status IN ('PRESENT', 'LATE', 'EARLY_LEAVE', 'HALF_DAY', 'ABSENT_WITH_PERMISSION', 'ABSENT_UNAUTHORIZED', 'HOLIDAY')),
    work_units DECIMAL(3, 2) NOT NULL DEFAULT 1.00 CHECK (work_units >= 0 AND work_units <= 2.0),
    is_locked BIT NOT NULL DEFAULT 0,
    attendance_source NVARCHAR(50) NOT NULL DEFAULT 'FACE_RECOGNITION',
    is_confirmed BIT NOT NULL DEFAULT 1,
    photo_in_url NVARCHAR(500) NULL,
    photo_out_url NVARCHAR(500) NULL,
    note NVARCHAR(255) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE UNIQUE INDEX uq_emp_work_date_shift ON timesheets(employee_id, work_date, shift_id) WHERE shift_id IS NOT NULL;
GO

CREATE TABLE attendance_photos (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    timesheet_id BIGINT NOT NULL CONSTRAINT fk_attendance_photos_ts REFERENCES timesheets(id) ON DELETE NO ACTION,
    employee_id BIGINT NOT NULL CONSTRAINT fk_attendance_photos_emp REFERENCES employees(id) ON DELETE NO ACTION,
    punch_type NVARCHAR(20) NOT NULL CHECK (punch_type IN ('CHECK_IN', 'CHECK_OUT')),
    photo_url NVARCHAR(500) NOT NULL,
    captured_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    notes NVARCHAR(500) NULL
);
GO

CREATE INDEX idx_attendance_photos_emp ON attendance_photos(employee_id, captured_at DESC);
CREATE INDEX idx_attendance_photos_ts ON attendance_photos(timesheet_id);
GO

CREATE TABLE leave_requests (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    employee_id BIGINT NOT NULL CONSTRAINT fk_leave_emp REFERENCES employees(id) ON DELETE CASCADE,
    leave_type NVARCHAR(50) NOT NULL CHECK (leave_type IN ('ANNUAL_LEAVE', 'SICK_LEAVE', 'MATERNITY_LEAVE', 'UNPAID_LEAVE')),
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    total_days DECIMAL(4, 2) NOT NULL CHECK (total_days > 0),
    reason NVARCHAR(MAX) NOT NULL,
    approval_status NVARCHAR(30) NOT NULL DEFAULT 'PENDING' CHECK (approval_status IN ('PENDING', 'APPROVED', 'REJECTED')),
    approved_by NVARCHAR(100) NULL,
    approved_at DATETIME2 NULL,
    reject_reason NVARCHAR(MAX) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT chk_leave_dates CHECK (end_date >= start_date)
);
GO

CREATE TABLE attendance_requests (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    employee_id BIGINT NOT NULL CONSTRAINT FK_attendance_requests_emp REFERENCES employees(id) ON DELETE CASCADE,
    work_date DATE NOT NULL,
    request_type NVARCHAR(50) NOT NULL,
    proposed_check_in TIME NULL,
    proposed_check_out TIME NULL,
    shift_id BIGINT NULL CONSTRAINT FK_attendance_requests_shifts REFERENCES work_shifts(id) ON DELETE SET NULL,
    reason NVARCHAR(500) NOT NULL,
    evidence_file_name NVARCHAR(255) NULL,
    approval_status NVARCHAR(50) NOT NULL DEFAULT 'PENDING',
    approved_by NVARCHAR(100) NULL,
    approved_at DATETIME2 NULL,
    rejection_reason NVARCHAR(500) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE employee_face_profiles (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    employee_id BIGINT NOT NULL UNIQUE CONSTRAINT fk_face_emp REFERENCES employees(id) ON DELETE CASCADE,
    face_descriptor_json NVARCHAR(MAX) NOT NULL,
    registered_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    registered_by NVARCHAR(100) NOT NULL,
    is_active BIT NOT NULL DEFAULT 1,
    liveness_threshold DECIMAL(5, 2) NOT NULL DEFAULT 0.70,
    notes NVARCHAR(500) NULL,
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE face_audit_logs (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    employee_id BIGINT NULL CONSTRAINT fk_faceaudit_emp REFERENCES employees(id) ON DELETE SET NULL,
    action_type NVARCHAR(50) NOT NULL,
    similarity_score DECIMAL(5, 4) NULL,
    performed_by NVARCHAR(100) NOT NULL,
    device_info NVARCHAR(255) NULL,
    is_success BIT NOT NULL DEFAULT 1,
    failure_reason NVARCHAR(500) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

CREATE TABLE timesheet_audit_logs (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    timesheet_id BIGINT NULL,
    employee_id BIGINT NOT NULL,
    action NVARCHAR(50) NOT NULL,
    before_data NVARCHAR(MAX) NULL,
    after_data NVARCHAR(MAX) NULL,
    modified_by NVARCHAR(100) NOT NULL,
    modified_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    reason NVARCHAR(500) NULL
);
GO

CREATE TABLE payslips (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    period_id BIGINT NOT NULL CONSTRAINT fk_payslips_period REFERENCES payroll_periods(id) ON DELETE CASCADE,
    employee_id BIGINT NOT NULL CONSTRAINT fk_payslips_emp REFERENCES employees(id) ON DELETE NO ACTION,
    actual_work_days DECIMAL(5, 2) NOT NULL DEFAULT 0.00 CHECK (actual_work_days >= 0),
    paid_leave_days DECIMAL(5, 2) NOT NULL DEFAULT 0.00 CHECK (paid_leave_days >= 0),
    overtime_hours DECIMAL(5, 2) NOT NULL DEFAULT 0.00 CHECK (overtime_hours >= 0),
    gross_salary DECIMAL(15, 2) NOT NULL DEFAULT 0.00 CHECK (gross_salary >= 0),
    total_deductions DECIMAL(15, 2) NOT NULL DEFAULT 0.00 CHECK (total_deductions >= 0),
    net_salary DECIMAL(15, 2) NOT NULL DEFAULT 0.00 CHECK (net_salary >= 0),
    note NVARCHAR(MAX) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT uq_period_employee UNIQUE (period_id, employee_id)
);
GO

CREATE TABLE payslip_lines (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    payslip_id BIGINT NOT NULL CONSTRAINT fk_lines_payslip REFERENCES payslips(id) ON DELETE CASCADE,
    line_type NVARCHAR(50) NOT NULL CHECK (line_type IN ('BASE_SALARY_PRORATED', 'PAID_LEAVE_PAY', 'MEAL_ALLOWANCE', 'FUEL_ALLOWANCE', 'OVERTIME_PAY', 'BONUS', 'SPLIT_SHIFT_ALLOWANCE', 'DEDUCTION_BHXH', 'DEDUCTION_BHYT', 'DEDUCTION_BHTN', 'PERSONAL_INCOME_TAX', 'PENALTY')),
    line_description NVARCHAR(255) NOT NULL,
    amount DECIMAL(15, 2) NOT NULL CHECK (amount >= 0),
    is_deduction BIT NOT NULL DEFAULT 0,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================================
-- BƯỚC 4: TẠO CHỈ MỤC TỐI ƯU HÓA HIỆU NĂNG (INDEXES)
-- ============================================================================
CREATE INDEX idx_employees_department ON employees(department_id);
CREATE INDEX idx_employees_status ON employees(status);
CREATE INDEX idx_user_accounts_username ON user_accounts(username);
CREATE INDEX idx_contracts_emp_active ON contracts(employee_id, is_active);
CREATE INDEX idx_timesheets_emp_date ON timesheets(employee_id, work_date);
CREATE INDEX idx_timesheets_period ON timesheets(period_id);
CREATE INDEX idx_leave_requests_emp_dates ON leave_requests(employee_id, start_date, end_date);
CREATE INDEX idx_attendance_requests_emp_date ON attendance_requests(employee_id, work_date);
CREATE INDEX idx_attendance_requests_status ON attendance_requests(approval_status);
CREATE INDEX idx_payslips_period_emp ON payslips(period_id, employee_id);
CREATE INDEX idx_payslips_employee ON payslips(employee_id);
CREATE INDEX idx_payslip_lines_payslip ON payslip_lines(payslip_id);
GO

-- ============================================================================
-- BƯỚC 5: NẠP DỮ LIỆU ĐẦY ĐỦ 20 NHÂN SỰ, 550+ CÔNG, YÊU CẦU CHẤM CÔNG & NGHỈ PHÉP
-- ============================================================================
USE BangChamCongDB;
GO

-- 1. THÊM CÁC PHÒNG BAN MỚI NẾU CHƯA CÓ
IF NOT EXISTS (SELECT 1 FROM departments WHERE department_code = 'DEP-OPS')
BEGIN
    INSERT INTO departments (department_code, department_name, manager_name, description)
    VALUES (N'DEP-OPS', N'Phòng Vận Hành & Dịch Vụ Khách Hàng', N'Hà Phương Thảo', N'Quản lý vận hành hàng ngày, tổng đài và dịch vụ khách hàng');
END

IF NOT EXISTS (SELECT 1 FROM departments WHERE department_code = 'DEP-FIN')
BEGIN
    INSERT INTO departments (department_code, department_name, manager_name, description)
    VALUES (N'DEP-FIN', N'Phòng Kế Toán Quản Trị & Thuế', N'Cao Thu Trang', N'Kiểm soát tài chính, lập báo cáo thuế và quản trị dòng tiền');
END

IF NOT EXISTS (SELECT 1 FROM departments WHERE department_code = 'DEP-TECH')
BEGIN
    INSERT INTO departments (department_code, department_name, manager_name, description)
    VALUES (N'DEP-TECH', N'Phòng Kỹ Thuật & Hạ Tầng Thiết Bị', N'Trịnh Quốc Bảo', N'Bảo trì máy móc, hạ tầng mạng CNTT và camera an ninh');
END

-- Đảm bảo đầy đủ 6 ca làm việc tiêu chuẩn và cấu hình cửa sổ chấm công
IF NOT EXISTS (SELECT 1 FROM work_shifts WHERE shift_code = 'CA_HANH_CHINH')
BEGIN
    INSERT INTO work_shifts (shift_code, shift_name, start_time, end_time, lunch_break_hours, target_hours, work_units, grace_period_minutes, early_checkin_minutes, late_checkout_minutes, is_night_shift, is_overnight, is_split_shift, split_shift_allowance, is_active, description)
    VALUES ('CA_HANH_CHINH', N'Ca Hành Chính (08:00 - 17:30)', '08:00:00', '17:30:00', 1.50, 8.00, 1.00, 15, 60, 120, 0, 0, 0, 0.00, 1, N'Ca làm việc tiêu chuẩn khối văn phòng');
END

IF NOT EXISTS (SELECT 1 FROM work_shifts WHERE shift_code = 'CA_SANG')
BEGIN
    INSERT INTO work_shifts (shift_code, shift_name, start_time, end_time, lunch_break_hours, target_hours, work_units, grace_period_minutes, early_checkin_minutes, late_checkout_minutes, is_night_shift, is_overnight, is_split_shift, split_shift_allowance, is_active, description)
    VALUES ('CA_SANG', N'Ca Sáng (08:00 - 12:00)', '08:00:00', '12:00:00', 0.00, 4.00, 0.50, 15, 60, 120, 0, 0, 0, 0.00, 1, N'Ca làm việc buổi sáng');
END

IF NOT EXISTS (SELECT 1 FROM work_shifts WHERE shift_code = 'CA_CHIEU')
BEGIN
    INSERT INTO work_shifts (shift_code, shift_name, start_time, end_time, lunch_break_hours, target_hours, work_units, grace_period_minutes, early_checkin_minutes, late_checkout_minutes, is_night_shift, is_overnight, is_split_shift, split_shift_allowance, is_active, description)
    VALUES ('CA_CHIEU', N'Ca Chiều (13:30 - 17:30)', '13:30:00', '17:30:00', 0.00, 4.00, 0.50, 15, 60, 120, 0, 0, 0, 0.00, 1, N'Ca làm việc buổi chiều');
END

IF NOT EXISTS (SELECT 1 FROM work_shifts WHERE shift_code = 'CA_TOI')
BEGIN
    INSERT INTO work_shifts (shift_code, shift_name, start_time, end_time, lunch_break_hours, target_hours, work_units, grace_period_minutes, early_checkin_minutes, late_checkout_minutes, is_night_shift, is_overnight, is_split_shift, split_shift_allowance, is_active, description)
    VALUES ('CA_TOI', N'Ca Tối (18:00 - 22:00)', '18:00:00', '22:00:00', 0.00, 4.00, 0.50, 15, 60, 120, 0, 0, 0, 0.00, 1, N'Ca làm việc buổi tối');
END

IF NOT EXISTS (SELECT 1 FROM work_shifts WHERE shift_code = 'CA_DEM')
BEGIN
    INSERT INTO work_shifts (shift_code, shift_name, start_time, end_time, lunch_break_hours, target_hours, work_units, grace_period_minutes, early_checkin_minutes, late_checkout_minutes, is_night_shift, is_overnight, is_split_shift, split_shift_allowance, is_active, description)
    VALUES ('CA_DEM', N'Ca Đêm (22:00 - 06:00)', '22:00:00', '06:00:00', 0.00, 8.00, 1.00, 15, 60, 120, 1, 1, 0, 0.00, 1, N'Ca làm việc qua đêm');
END

IF NOT EXISTS (SELECT 1 FROM work_shifts WHERE shift_code = 'CA_GAY')
BEGIN
    INSERT INTO work_shifts (shift_code, shift_name, start_time, end_time, lunch_break_hours, target_hours, work_units, grace_period_minutes, early_checkin_minutes, late_checkout_minutes, is_night_shift, is_overnight, is_split_shift, split_shift_allowance, is_active, description)
    VALUES ('CA_GAY', N'Ca Gãy (10:00 - 14:00 & 17:00 - 21:00)', '10:00:00', '21:00:00', 3.00, 8.00, 1.00, 15, 60, 120, 0, 0, 1, 50000.00, 1, N'Ca gãy đặc thù dịch vụ, ca kíp (nghỉ giữa 3h, phụ cấp ca gãy 50.000đ/ca)');
END
GO

-- 2. THÊM 15 NHÂN VIÊN MỚI (EMP006 ĐẾN EMP020)
DECLARE @DeptIT BIGINT = (SELECT TOP 1 id FROM departments WHERE department_code = 'DEP-IT');
DECLARE @DeptHR BIGINT = (SELECT TOP 1 id FROM departments WHERE department_code = 'DEP-HR');
DECLARE @DeptSales BIGINT = (SELECT TOP 1 id FROM departments WHERE department_code = 'DEP-SALES');
DECLARE @DeptOps BIGINT = (SELECT TOP 1 id FROM departments WHERE department_code = 'DEP-OPS');
DECLARE @DeptFin BIGINT = (SELECT TOP 1 id FROM departments WHERE department_code = 'DEP-FIN');
DECLARE @DeptTech BIGINT = (SELECT TOP 1 id FROM departments WHERE department_code = 'DEP-TECH');

IF NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'EMP006')
INSERT INTO employees (employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status)
VALUES (N'EMP006', N'Đỗ Thị Mai Linh', '1996-04-12', N'Nữ', N'001096001234', N'linh.dtm@congty.com', N'0912111222', N'19036667778881', N'Vietcombank', @DeptHR, '2023-04-01', 'ACTIVE');

IF NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'EMP007')
INSERT INTO employees (employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status)
VALUES (N'EMP007', N'Vũ Quốc Anh', '1993-09-25', N'Nam', N'001093005678', N'anh.vq@congty.com', N'0913222333', N'19036667778882', N'Techcombank', @DeptIT, '2022-08-15', 'ACTIVE');

IF NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'EMP008')
INSERT INTO employees (employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status)
VALUES (N'EMP008', N'Bùi Thị Bích Ngọc', '1991-12-05', N'Nữ', N'001091009012', N'ngoc.btb@congty.com', N'0914333444', N'19036667778883', N'MB Bank', @DeptSales, '2021-11-01', 'ACTIVE');

IF NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'EMP009')
INSERT INTO employees (employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status)
VALUES (N'EMP009', N'Đinh Quang Huy', '1997-07-18', N'Nam', N'001097003456', N'huy.dq@congty.com', N'0915444555', N'19036667778884', N'BIDV', @DeptOps, '2023-09-10', 'ACTIVE');

IF NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'EMP010')
INSERT INTO employees (employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status)
VALUES (N'EMP010', N'Nguyễn Hải Yến', '1994-03-30', N'Nữ', N'001094007890', N'yen.nh@congty.com', N'0916555666', N'19036667778885', N'VietinBank', @DeptFin, '2022-05-20', 'ACTIVE');

IF NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'EMP011')
INSERT INTO employees (employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status)
VALUES (N'EMP011', N'Phạm Minh Đức', '1995-10-14', N'Nam', N'001095002345', N'duc.pm@congty.com', N'0917666777', N'19036667778886', N'ACB', @DeptTech, '2023-02-15', 'ACTIVE');

IF NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'EMP012')
INSERT INTO employees (employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status)
VALUES (N'EMP012', N'Lương Kim Ngân', '1996-01-22', N'Nữ', N'001096006789', N'ngan.lk@congty.com', N'0918777888', N'19036667778887', N'Vietcombank', @DeptHR, '2023-06-01', 'ACTIVE');

IF NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'EMP013')
INSERT INTO employees (employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status)
VALUES (N'EMP013', N'Tạ Văn Kiên', '1998-08-08', N'Nam', N'001098001122', N'kien.tv@congty.com', N'0919888999', N'19036667778888', N'TPBank', @DeptSales, '2024-01-05', 'ACTIVE');

IF NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'EMP014')
INSERT INTO employees (employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status)
VALUES (N'EMP014', N'Hà Phương Thảo', '1990-06-17', N'Nữ', N'001090004455', N'thao.hp@congty.com', N'0920999000', N'19036667778889', N'Techcombank', @DeptOps, '2020-10-15', 'ACTIVE');

IF NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'EMP015')
INSERT INTO employees (employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status)
VALUES (N'EMP015', N'Ngô Thanh Tùng', '1992-11-28', N'Nam', N'001092008899', N'tung.nt@congty.com', N'0921000111', N'19036667778890', N'Vietcombank', @DeptIT, '2021-04-10', 'ACTIVE');

IF NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'EMP016')
INSERT INTO employees (employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status)
VALUES (N'EMP016', N'Dương Mỹ Hạnh', '1999-05-03', N'Nữ', N'001099003344', N'hanh.dm@congty.com', N'0922111222', N'19036667778891', N'MB Bank', @DeptSales, '2024-03-01', 'ACTIVE');

IF NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'EMP017')
INSERT INTO employees (employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status)
VALUES (N'EMP017', N'Trịnh Quốc Bảo', '1987-02-14', N'Nam', N'001087007788', N'bao.tq@congty.com', N'0923222333', N'19036667778892', N'BIDV', @DeptTech, '2019-07-01', 'ACTIVE');

IF NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'EMP018')
INSERT INTO employees (employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status)
VALUES (N'EMP018', N'Cao Thu Trang', '1989-12-19', N'Nữ', N'001089002233', N'trang.ct@congty.com', N'0924333444', N'19036667778893', N'VietinBank', @DeptFin, '2020-03-15', 'ACTIVE');

IF NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'EMP019')
INSERT INTO employees (employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status)
VALUES (N'EMP019', N'Mai Hoàng Long', '1997-09-09', N'Nam', N'001097006677', N'long.mh@congty.com', N'0925444555', N'19036667778894', N'VPBank', @DeptOps, '2023-11-20', 'ACTIVE');

IF NOT EXISTS (SELECT 1 FROM employees WHERE employee_code = 'EMP020')
INSERT INTO employees (employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status)
VALUES (N'EMP020', N'Võ Như Quỳnh', '1996-08-23', N'Nữ', N'001096009900', N'quynh.vn@congty.com', N'0926555666', N'19036667778895', N'ACB', @DeptIT, '2022-12-01', 'ACTIVE');
GO

-- 3. TẠO TÀI KHOẢN ĐĂNG NHẬP CHO CÁC NHÂN VIÊN MỚI (Pass mặc định 123456)
DECLARE @PashHash NVARCHAR(255) = N'$2a$10$EblZqNptyYvcLm/VwDCVAuBjzZOI7khzdyGPBr08PpIi0na624b8.';

INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active)
SELECT e.id, 'nhanvien.linh', @PashHash, 'ROLE_EMPLOYEE', 1 FROM employees e WHERE e.employee_code = 'EMP006' AND NOT EXISTS (SELECT 1 FROM user_accounts u WHERE u.employee_id = e.id);

INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active)
SELECT e.id, 'nhanvien.anh', @PashHash, 'ROLE_EMPLOYEE', 1 FROM employees e WHERE e.employee_code = 'EMP007' AND NOT EXISTS (SELECT 1 FROM user_accounts u WHERE u.employee_id = e.id);

INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active)
SELECT e.id, 'quanly.ngoc', @PashHash, 'ROLE_MANAGER', 1 FROM employees e WHERE e.employee_code = 'EMP008' AND NOT EXISTS (SELECT 1 FROM user_accounts u WHERE u.employee_id = e.id);

INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active)
SELECT e.id, 'nhanvien.huy', @PashHash, 'ROLE_EMPLOYEE', 1 FROM employees e WHERE e.employee_code = 'EMP009' AND NOT EXISTS (SELECT 1 FROM user_accounts u WHERE u.employee_id = e.id);

INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active)
SELECT e.id, 'ketoan.yen', @PashHash, 'ROLE_HR_ACCOUNTANT', 1 FROM employees e WHERE e.employee_code = 'EMP010' AND NOT EXISTS (SELECT 1 FROM user_accounts u WHERE u.employee_id = e.id);

INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active)
SELECT e.id, 'nhanvien.duc', @PashHash, 'ROLE_EMPLOYEE', 1 FROM employees e WHERE e.employee_code = 'EMP011' AND NOT EXISTS (SELECT 1 FROM user_accounts u WHERE u.employee_id = e.id);

INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active)
SELECT e.id, 'ketoan.ngan', @PashHash, 'ROLE_HR_ACCOUNTANT', 1 FROM employees e WHERE e.employee_code = 'EMP012' AND NOT EXISTS (SELECT 1 FROM user_accounts u WHERE u.employee_id = e.id);

INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active)
SELECT e.id, 'nhanvien.kien', @PashHash, 'ROLE_EMPLOYEE', 1 FROM employees e WHERE e.employee_code = 'EMP013' AND NOT EXISTS (SELECT 1 FROM user_accounts u WHERE u.employee_id = e.id);

INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active)
SELECT e.id, 'quanly.thao', @PashHash, 'ROLE_MANAGER', 1 FROM employees e WHERE e.employee_code = 'EMP014' AND NOT EXISTS (SELECT 1 FROM user_accounts u WHERE u.employee_id = e.id);

INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active)
SELECT e.id, 'nhanvien.tung', @PashHash, 'ROLE_EMPLOYEE', 1 FROM employees e WHERE e.employee_code = 'EMP015' AND NOT EXISTS (SELECT 1 FROM user_accounts u WHERE u.employee_id = e.id);

INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active)
SELECT e.id, 'nhanvien.hanh', @PashHash, 'ROLE_EMPLOYEE', 1 FROM employees e WHERE e.employee_code = 'EMP016' AND NOT EXISTS (SELECT 1 FROM user_accounts u WHERE u.employee_id = e.id);

INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active)
SELECT e.id, 'quanly.bao', @PashHash, 'ROLE_MANAGER', 1 FROM employees e WHERE e.employee_code = 'EMP017' AND NOT EXISTS (SELECT 1 FROM user_accounts u WHERE u.employee_id = e.id);

INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active)
SELECT e.id, 'ketoan.trang', @PashHash, 'ROLE_HR_ACCOUNTANT', 1 FROM employees e WHERE e.employee_code = 'EMP018' AND NOT EXISTS (SELECT 1 FROM user_accounts u WHERE u.employee_id = e.id);

INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active)
SELECT e.id, 'nhanvien.long', @PashHash, 'ROLE_EMPLOYEE', 1 FROM employees e WHERE e.employee_code = 'EMP019' AND NOT EXISTS (SELECT 1 FROM user_accounts u WHERE u.employee_id = e.id);

INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active)
SELECT e.id, 'nhanvien.quynh', @PashHash, 'ROLE_EMPLOYEE', 1 FROM employees e WHERE e.employee_code = 'EMP020' AND NOT EXISTS (SELECT 1 FROM user_accounts u WHERE u.employee_id = e.id);
GO

-- 4. TẠO HỢP ĐỒNG LAO ĐỘNG CHO CÁC NHÂN VIÊN MỚI
INSERT INTO contracts (employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active)
SELECT e.id, 'HDLD-2023-006', 'INDEFINITE', 12000000.00, 500000.00, 300000.00, '2023-04-01', NULL, 1 FROM employees e WHERE e.employee_code = 'EMP006' AND NOT EXISTS (SELECT 1 FROM contracts c WHERE c.employee_id = e.id);

INSERT INTO contracts (employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active)
SELECT e.id, 'HDLD-2022-007', 'INDEFINITE', 18000000.00, 500000.00, 500000.00, '2022-08-15', NULL, 1 FROM employees e WHERE e.employee_code = 'EMP007' AND NOT EXISTS (SELECT 1 FROM contracts c WHERE c.employee_id = e.id);

INSERT INTO contracts (employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active)
SELECT e.id, 'HDLD-2021-008B', 'INDEFINITE', 16000000.00, 500000.00, 1000000.00, '2021-11-01', NULL, 1 FROM employees e WHERE e.employee_code = 'EMP008' AND NOT EXISTS (SELECT 1 FROM contracts c WHERE c.employee_id = e.id);

INSERT INTO contracts (employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active)
SELECT e.id, 'HDLD-2023-009', 'INDEFINITE', 11000000.00, 500000.00, 400000.00, '2023-09-10', NULL, 1 FROM employees e WHERE e.employee_code = 'EMP009' AND NOT EXISTS (SELECT 1 FROM contracts c WHERE c.employee_id = e.id);

INSERT INTO contracts (employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active)
SELECT e.id, 'HDLD-2022-010', 'INDEFINITE', 14000000.00, 500000.00, 500000.00, '2022-05-20', NULL, 1 FROM employees e WHERE e.employee_code = 'EMP010' AND NOT EXISTS (SELECT 1 FROM contracts c WHERE c.employee_id = e.id);

INSERT INTO contracts (employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active)
SELECT e.id, 'HDLD-2023-011', 'INDEFINITE', 13000000.00, 500000.00, 500000.00, '2023-02-15', NULL, 1 FROM employees e WHERE e.employee_code = 'EMP011' AND NOT EXISTS (SELECT 1 FROM contracts c WHERE c.employee_id = e.id);

INSERT INTO contracts (employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active)
SELECT e.id, 'HDLD-2023-012', 'INDEFINITE', 13500000.00, 500000.00, 300000.00, '2023-06-01', NULL, 1 FROM employees e WHERE e.employee_code = 'EMP012' AND NOT EXISTS (SELECT 1 FROM contracts c WHERE c.employee_id = e.id);

INSERT INTO contracts (employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active)
SELECT e.id, 'HDLD-2024-013', 'FULL_TIME', 10500000.00, 500000.00, 600000.00, '2024-01-05', '2025-01-05', 1 FROM employees e WHERE e.employee_code = 'EMP013' AND NOT EXISTS (SELECT 1 FROM contracts c WHERE c.employee_id = e.id);

INSERT INTO contracts (employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active)
SELECT e.id, 'HDLD-2020-014', 'INDEFINITE', 15000000.00, 500000.00, 800000.00, '2020-10-15', NULL, 1 FROM employees e WHERE e.employee_code = 'EMP014' AND NOT EXISTS (SELECT 1 FROM contracts c WHERE c.employee_id = e.id);

INSERT INTO contracts (employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active)
SELECT e.id, 'HDLD-2021-015', 'INDEFINITE', 22000000.00, 500000.00, 500000.00, '2021-04-10', NULL, 1 FROM employees e WHERE e.employee_code = 'EMP015' AND NOT EXISTS (SELECT 1 FROM contracts c WHERE c.employee_id = e.id);

INSERT INTO contracts (employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active)
SELECT e.id, 'HDLD-2024-016', 'FULL_TIME', 9500000.00, 500000.00, 400000.00, '2024-03-01', '2025-03-01', 1 FROM employees e WHERE e.employee_code = 'EMP016' AND NOT EXISTS (SELECT 1 FROM contracts c WHERE c.employee_id = e.id);

INSERT INTO contracts (employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active)
SELECT e.id, 'HDLD-2019-017', 'INDEFINITE', 20000000.00, 500000.00, 1000000.00, '2019-07-01', NULL, 1 FROM employees e WHERE e.employee_code = 'EMP017' AND NOT EXISTS (SELECT 1 FROM contracts c WHERE c.employee_id = e.id);

INSERT INTO contracts (employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active)
SELECT e.id, 'HDLD-2020-018', 'INDEFINITE', 17000000.00, 500000.00, 600000.00, '2020-03-15', NULL, 1 FROM employees e WHERE e.employee_code = 'EMP018' AND NOT EXISTS (SELECT 1 FROM contracts c WHERE c.employee_id = e.id);

INSERT INTO contracts (employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active)
SELECT e.id, 'HDLD-2023-019', 'INDEFINITE', 10000000.00, 500000.00, 400000.00, '2023-11-20', NULL, 1 FROM employees e WHERE e.employee_code = 'EMP019' AND NOT EXISTS (SELECT 1 FROM contracts c WHERE c.employee_id = e.id);

INSERT INTO contracts (employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active)
SELECT e.id, 'HDLD-2022-020', 'INDEFINITE', 13000000.00, 500000.00, 500000.00, '2022-12-01', NULL, 1 FROM employees e WHERE e.employee_code = 'EMP020' AND NOT EXISTS (SELECT 1 FROM contracts c WHERE c.employee_id = e.id);
GO

-- 5. SINH BẢNG CHẤM CÔNG TOÀN DIỆN CHO CẢ 20 NHÂN VIÊN TRONG THÁNG 10/2026 (TỪ 01/10 ĐẾN 31/10)
DECLARE @PeriodId BIGINT = (SELECT TOP 1 id FROM payroll_periods WHERE period_code = 'PR-2026-10');
IF @PeriodId IS NULL
BEGIN
    INSERT INTO payroll_periods (period_code, period_month, period_year, standard_work_days, status, total_gross_amount, total_net_amount)
    VALUES ('PR-2026-10', 10, 2026, 22.00, 'CALCULATED', 0, 0);
    SET @PeriodId = SCOPE_IDENTITY();
END

DECLARE @ShiftHanhChinhId BIGINT = ISNULL((SELECT TOP 1 id FROM work_shifts WHERE shift_code = 'CA_HANH_CHINH'), 1);
DECLARE @ShiftSangId BIGINT = ISNULL((SELECT TOP 1 id FROM work_shifts WHERE shift_code = 'CA_SANG'), 2);
DECLARE @ShiftDemId BIGINT = ISNULL((SELECT TOP 1 id FROM work_shifts WHERE shift_code = 'CA_DEM'), 5);
DECLARE @ShiftGayId BIGINT = ISNULL((SELECT TOP 1 id FROM work_shifts WHERE shift_code = 'CA_GAY'), 1);

-- Bảng số ngày từ 1 đến 31 tháng 10 năm 2026
;WITH DaysMonth AS (
    SELECT CAST('2026-10-01' AS DATE) AS WorkDate
    UNION ALL
    SELECT DATEADD(DAY, 1, WorkDate)
    FROM DaysMonth
    WHERE WorkDate < '2026-10-31'
),
EmpWorkDays AS (
    SELECT 
        e.id AS EmployeeId,
        e.employee_code AS EmpCode,
        d.WorkDate,
        DATEPART(WEEKDAY, d.WorkDate) AS DayOfWeekNum -- 1: Sunday, 2: Monday... 7: Saturday
    FROM employees e
    CROSS JOIN DaysMonth d
    WHERE d.WorkDate NOT IN (SELECT t.work_date FROM timesheets t WHERE t.employee_id = e.id)
      AND DATEPART(WEEKDAY, d.WorkDate) <> 1 -- Bỏ qua Chủ Nhật
)
INSERT INTO timesheets (employee_id, period_id, shift_id, shift_code, shift_name, work_date, check_in_time, check_out_time, actual_hours_worked, work_shift_status, work_units, is_locked, note)
SELECT 
    ew.EmployeeId,
    @PeriodId,
    CASE 
        WHEN ew.EmpCode IN ('EMP004', 'EMP009', 'EMP019') AND ew.DayOfWeekNum = 7 THEN @ShiftGayId -- Ca gãy thứ 7
        WHEN ew.EmpCode IN ('EMP011', 'EMP017') AND ew.DayOfWeekNum = 6 THEN @ShiftDemId -- Ca đêm thứ 6
        WHEN ew.DayOfWeekNum = 7 THEN @ShiftSangId -- Ca sáng thứ 7
        ELSE @ShiftHanhChinhId -- Ca hành chính
    END AS shift_id,
    CASE 
        WHEN ew.EmpCode IN ('EMP004', 'EMP009', 'EMP019') AND ew.DayOfWeekNum = 7 THEN 'CA_GAY'
        WHEN ew.EmpCode IN ('EMP011', 'EMP017') AND ew.DayOfWeekNum = 6 THEN 'CA_DEM'
        WHEN ew.DayOfWeekNum = 7 THEN 'CA_SANG'
        ELSE 'CA_HANH_CHINH'
    END AS shift_code,
    CASE 
        WHEN ew.EmpCode IN ('EMP004', 'EMP009', 'EMP019') AND ew.DayOfWeekNum = 7 THEN N'Ca Gãy (10:00 - 14:00 & 17:00 - 21:00)'
        WHEN ew.EmpCode IN ('EMP011', 'EMP017') AND ew.DayOfWeekNum = 6 THEN N'Ca Đêm (22:00 - 06:00)'
        WHEN ew.DayOfWeekNum = 7 THEN N'Ca Sáng (08:00 - 12:00)'
        ELSE N'Ca Hành Chính (08:00 - 17:30)'
    END AS shift_name,
    ew.WorkDate,
    -- Check in time
    CASE 
        WHEN ew.WorkDate IN ('2026-10-15', '2026-10-21', '2026-10-22') AND ew.EmpCode IN ('EMP001', 'EMP006', 'EMP013') THEN NULL
        WHEN ew.EmpCode IN ('EMP004', 'EMP009', 'EMP019') AND ew.DayOfWeekNum = 7 THEN '10:00:00'
        WHEN ew.EmpCode IN ('EMP011', 'EMP017') AND ew.DayOfWeekNum = 6 THEN '22:00:00'
        WHEN DAY(ew.WorkDate) % 7 = 0 THEN '08:22:00' -- Đi muộn nhẹ một số ngày
        ELSE '07:55:00'
    END AS check_in_time,
    -- Check out time
    CASE 
        WHEN ew.WorkDate IN ('2026-10-15', '2026-10-21', '2026-10-22') AND ew.EmpCode IN ('EMP001', 'EMP006', 'EMP013') THEN NULL
        WHEN ew.EmpCode IN ('EMP004', 'EMP009', 'EMP019') AND ew.DayOfWeekNum = 7 THEN '21:00:00'
        WHEN ew.EmpCode IN ('EMP011', 'EMP017') AND ew.DayOfWeekNum = 6 THEN '06:00:00'
        WHEN ew.DayOfWeekNum = 7 THEN '12:00:00'
        WHEN DAY(ew.WorkDate) % 9 = 0 THEN '16:45:00' -- Về sớm nhẹ một số ngày
        ELSE '17:30:00'
    END AS check_out_time,
    -- Actual hours
    CASE 
        WHEN ew.WorkDate IN ('2026-10-15', '2026-10-21', '2026-10-22') AND ew.EmpCode IN ('EMP001', 'EMP006', 'EMP013') THEN 0.0
        WHEN ew.DayOfWeekNum = 7 AND ew.EmpCode NOT IN ('EMP004', 'EMP009', 'EMP019') THEN 4.0
        WHEN DAY(ew.WorkDate) % 7 = 0 THEN 7.6
        WHEN DAY(ew.WorkDate) % 9 = 0 THEN 7.25
        ELSE 8.0
    END AS actual_hours_worked,
    -- Work shift status
    CASE 
        WHEN ew.WorkDate IN ('2026-10-15', '2026-10-21', '2026-10-22') AND ew.EmpCode IN ('EMP001', 'EMP006', 'EMP013') THEN 'ABSENT_WITH_PERMISSION'
        WHEN DAY(ew.WorkDate) % 7 = 0 THEN 'LATE'
        WHEN DAY(ew.WorkDate) % 9 = 0 THEN 'EARLY_LEAVE'
        WHEN ew.DayOfWeekNum = 7 AND ew.EmpCode NOT IN ('EMP004', 'EMP009', 'EMP019') THEN 'HALF_DAY'
        ELSE 'PRESENT'
    END AS work_shift_status,
    -- Work units
    CASE 
        WHEN ew.WorkDate IN ('2026-10-15', '2026-10-21', '2026-10-22') AND ew.EmpCode IN ('EMP001', 'EMP006', 'EMP013') THEN 0.0
        WHEN ew.DayOfWeekNum = 7 AND ew.EmpCode NOT IN ('EMP004', 'EMP009', 'EMP019') THEN 0.5
        ELSE 1.0
    END AS work_units,
    0 AS is_locked,
    CASE 
        WHEN ew.WorkDate IN ('2026-10-15', '2026-10-21', '2026-10-22') AND ew.EmpCode IN ('EMP001', 'EMP006', 'EMP013') THEN N'Nghỉ phép có đơn phê duyệt'
        WHEN DAY(ew.WorkDate) % 7 = 0 THEN N'Đi muộn do điều kiện thời tiết/giao thông'
        WHEN DAY(ew.WorkDate) % 9 = 0 THEN N'Về sớm có thông báo trước'
        ELSE N'Đúng giờ tiêu chuẩn'
    END AS note
FROM EmpWorkDays ew;
GO

-- 6. THÊM 15 YÊU CẦU CHẤM CÔNG THỦ CÔNG (ATTENDANCE REQUESTS) ĐA DẠNG
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'attendance_requests')
BEGIN
    -- Xóa bớt để nạp bộ mới đẹp đẽ
    DELETE FROM attendance_requests;

    DECLARE @ShiftGayId BIGINT = ISNULL((SELECT TOP 1 id FROM work_shifts WHERE shift_code = 'CA_GAY'), 1);

    INSERT INTO attendance_requests (employee_id, work_date, request_type, proposed_check_in, proposed_check_out, shift_id, reason, approval_status, approved_by, approved_at, rejection_reason, created_at)
    VALUES
    -- Yêu cầu chờ duyệt (PENDING) để Admin/Manager thao tác duyệt/từ chối
    (6, '2026-10-23', 'CHECK_IN_MISSING', '08:00:00', '17:30:00', 1, N'Quên quẹt thẻ khi vào ca sáng do vào họp khẩn với phòng nhân sự', 'PENDING', NULL, NULL, NULL, '2026-10-23 08:30:00'),
    (7, '2026-10-23', 'CHECK_OUT_MISSING', '08:00:00', '17:30:00', 1, N'Cổng từ bảo vệ mất điện lúc 17h30 không quẹt ra được', 'PENDING', NULL, NULL, NULL, '2026-10-23 18:00:00'),
    (8, '2026-10-24', 'BOTH_ADJUSTMENT', '08:00:00', '18:30:00', 1, N'Đi công tác gặp gỡ khách hàng đối tác tại khu công nghiệp cả ngày', 'PENDING', NULL, NULL, NULL, '2026-10-24 19:00:00'),
    (9, '2026-10-24', 'CHECK_IN_MISSING', '10:00:00', '21:00:00', @ShiftGayId, N'Hỗ trợ xử lý sự cố thiết bị tại cơ sở lúc 10h sáng quên chấm công', 'PENDING', NULL, NULL, NULL, '2026-10-24 11:00:00'),
    (11, '2026-10-25', 'CHECK_OUT_MISSING', '08:00:00', '17:30:00', 1, N'Hoàn thành bảo trì hệ thống cáp mạng đến cuối giờ vội ra xe', 'PENDING', NULL, NULL, NULL, '2026-10-25 18:10:00'),
    (13, '2026-10-25', 'BOTH_ADJUSTMENT', '08:00:00', '17:30:00', 1, N'Tham gia hội chợ triển lãm xúc tiến thương mại tại trung tâm hội nghị', 'PENDING', NULL, NULL, NULL, '2026-10-25 17:45:00'),

    -- Yêu cầu đã duyệt (APPROVED)
    (1, '2026-10-10', 'CHECK_IN_MISSING', '08:00:00', '17:30:00', 1, N'Gặp trục trặc ứng dụng nhận diện khuôn mặt lúc vào', 'APPROVED', N'Trần Đình Trọng', '2026-10-10 10:00:00', NULL, '2026-10-10 08:15:00'),
    (4, '2026-10-12', 'BOTH_ADJUSTMENT', '10:00:00', '21:00:00', @ShiftGayId, N'Làm ca gãy hỗ trợ sự kiện khách hàng đã có xác nhận của trưởng nhóm', 'APPROVED', N'Trần Đình Trọng', '2026-10-13 09:00:00', NULL, '2026-10-12 21:30:00'),
    (10, '2026-10-16', 'CHECK_OUT_MISSING', '08:00:00', '17:30:00', 1, N'Chốt sổ sách cuối ngày quên quẹt thẻ lúc ra', 'APPROVED', N'Lê Thị Thu Thảo', '2026-10-17 08:30:00', NULL, '2026-10-16 18:00:00'),
    (12, '2026-10-18', 'CHECK_IN_MISSING', '08:00:00', '17:30:00', 1, N'Trực tiếp đi nộp hồ sơ bảo hiểm tại cơ quan BHXH', 'APPROVED', N'Lê Thị Thu Thảo', '2026-10-18 11:00:00', NULL, '2026-10-18 08:30:00'),
    (15, '2026-10-19', 'BOTH_ADJUSTMENT', '08:00:00', '17:30:00', 1, N'Bảo trì máy chủ trung tâm dữ liệu ngoài giờ', 'APPROVED', N'Trần Đình Trọng', '2026-10-20 09:15:00', NULL, '2026-10-19 20:00:00'),

    -- Yêu cầu bị từ chối (REJECTED) kèm lý do rõ ràng
    (16, '2026-10-08', 'BOTH_ADJUSTMENT', '08:00:00', '17:30:00', 1, N'Không chấm công cả ngày do bận việc cá nhân', 'REJECTED', N'Trần Đình Trọng', '2026-10-09 09:00:00', N'Lý do việc cá nhân không đủ điều kiện chấm bù công ty, vui lòng làm đơn nghỉ phép.', '2026-10-08 18:00:00'),
    (19, '2026-10-14', 'CHECK_IN_MISSING', '07:30:00', '17:30:00', 1, N'Đề nghị tính đi làm từ 7h30', 'REJECTED', N'Trần Đình Trọng', '2026-10-15 08:45:00', N'Ca làm việc bắt đầu từ 8h00, hệ thống không duyệt tăng giờ trước ca khi chưa có đăng ký OT.', '2026-10-14 19:30:00');
END
GO

-- 7. THÊM CÁC ĐƠN NGHỈ PHÉP (LEAVE REQUESTS) ĐA DẠNG
DELETE FROM leave_requests WHERE id > 3;

INSERT INTO leave_requests (employee_id, leave_type, start_date, end_date, total_days, reason, approval_status, approved_by, approved_at, reject_reason, created_at)
VALUES
-- Đơn chờ duyệt (PENDING)
(6, 'ANNUAL_LEAVE', '2026-10-28', '2026-10-29', 2.0, N'Nghỉ phép thường niên đi du lịch cùng gia đình', 'PENDING', NULL, NULL, NULL, '2026-10-24 10:00:00'),
(8, 'SICK_LEAVE', '2026-10-27', '2026-10-27', 1.0, N'Khám sức khỏe tổng quát định kỳ tại bệnh viện', 'PENDING', NULL, NULL, NULL, '2026-10-24 14:00:00'),
(11, 'UNPAID_LEAVE', '2026-10-30', '2026-10-30', 1.0, N'Giải quyết việc cá nhân tại địa phương', 'PENDING', NULL, NULL, NULL, '2026-10-25 09:00:00'),
(14, 'ANNUAL_LEAVE', '2026-10-29', '2026-10-31', 3.0, N'Nghỉ phép cưới em gái ruột', 'PENDING', NULL, NULL, NULL, '2026-10-25 11:30:00'),

-- Đơn đã duyệt (APPROVED)
(7, 'ANNUAL_LEAVE', '2026-10-06', '2026-10-06', 1.0, N'Nghỉ phép thường niên', 'APPROVED', N'Trần Đình Trọng', '2026-10-05 16:00:00', NULL, '2026-10-04 10:00:00'),
(10, 'SICK_LEAVE', '2026-10-15', '2026-10-15', 1.0, N'Sốt xuất huyết theo chỉ định bác sĩ', 'APPROVED', N'Lê Thị Thu Thảo', '2026-10-14 17:00:00', NULL, '2026-10-14 08:30:00'),

-- Đơn từ chối (REJECTED) có lý do
(13, 'ANNUAL_LEAVE', '2026-10-20', '2026-10-24', 5.0, N'Nghỉ đột xuất 5 ngày liên tục', 'REJECTED', N'Trần Đình Trọng', '2026-10-19 14:00:00', N'Trùng đợt cao điểm chiến dịch bán hàng cuối tháng, vui lòng dời lịch nghỉ sang tháng sau.', '2026-10-18 10:00:00');
GO

-- 8. TÍNH TOÁN BẢNG LƯƠNG & DÒNG LƯƠNG CHI TIẾT (PAYSLIPS & PAYSLIP LINES) CHO TẤT CẢ 20 NHÂN VIÊN
DECLARE @PeriodId BIGINT = (SELECT TOP 1 id FROM payroll_periods WHERE period_code = 'PR-2026-10');

-- Tạo hoặc cập nhật payslips cho tất cả nhân viên
MERGE INTO payslips AS target
USING (
    SELECT 
        e.id AS employee_id,
        @PeriodId AS period_id,
        ISNULL(SUM(CASE WHEN t.work_shift_status IN ('PRESENT', 'LATE', 'EARLY_LEAVE') THEN t.work_units ELSE 0 END), 22.0) AS actual_work_days,
        ISNULL(SUM(CASE WHEN t.work_shift_status = 'ABSENT_WITH_PERMISSION' THEN 1.0 ELSE 0 END), 0.0) AS paid_leave_days,
        0.0 AS overtime_hours,
        -- Gross salary = (BaseSalary / 22 * work_days) + Meal + Fuel
        ROUND((ISNULL(c.base_salary, 10000000) / 22.0 * ISNULL(SUM(CASE WHEN t.work_shift_status IN ('PRESENT', 'LATE', 'EARLY_LEAVE') THEN t.work_units ELSE 0 END), 22.0))
              + ISNULL(c.meal_allowance, 500000) + ISNULL(c.fuel_allowance, 0), 0) AS gross_salary,
        -- Total deductions = (BHXH 8% + BHYT 1.5% + BHTN 1% = 10.5% * BaseSalary)
        ROUND(ISNULL(c.base_salary, 10000000) * 0.105, 0) AS total_deductions,
        -- Net salary = Gross - Deductions
        ROUND(((ISNULL(c.base_salary, 10000000) / 22.0 * ISNULL(SUM(CASE WHEN t.work_shift_status IN ('PRESENT', 'LATE', 'EARLY_LEAVE') THEN t.work_units ELSE 0 END), 22.0))
              + ISNULL(c.meal_allowance, 500000) + ISNULL(c.fuel_allowance, 0)) - (ISNULL(c.base_salary, 10000000) * 0.105), 0) AS net_salary
    FROM employees e
    LEFT JOIN contracts c ON e.id = c.employee_id AND c.is_active = 1
    LEFT JOIN timesheets t ON e.id = t.employee_id AND t.period_id = @PeriodId
    GROUP BY e.id, c.base_salary, c.meal_allowance, c.fuel_allowance
) AS source
ON target.period_id = source.period_id AND target.employee_id = source.employee_id
WHEN MATCHED THEN
    UPDATE SET 
        target.actual_work_days = source.actual_work_days,
        target.paid_leave_days = source.paid_leave_days,
        target.gross_salary = source.gross_salary,
        target.total_deductions = source.total_deductions,
        target.net_salary = source.net_salary,
        target.updated_at = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (period_id, employee_id, actual_work_days, paid_leave_days, overtime_hours, gross_salary, total_deductions, net_salary)
    VALUES (source.period_id, source.employee_id, source.actual_work_days, source.paid_leave_days, source.overtime_hours, source.gross_salary, source.total_deductions, source.net_salary);

-- Sinh payslip_lines cho các payslips chưa có dòng chi tiết
INSERT INTO payslip_lines (payslip_id, line_type, line_description, amount, is_deduction)
SELECT p.id, 'BASE_SALARY_PRORATED', N'Lương ngày công thực tế (' + CAST(CAST(p.actual_work_days AS INT) AS NVARCHAR) + N'/22 ngày công)', 
       ROUND(ISNULL(c.base_salary, 10000000) / 22.0 * p.actual_work_days, 0), 0
FROM payslips p
JOIN employees e ON p.employee_id = e.id
LEFT JOIN contracts c ON e.id = c.employee_id AND c.is_active = 1
WHERE p.period_id = @PeriodId AND NOT EXISTS (SELECT 1 FROM payslip_lines pl WHERE pl.payslip_id = p.id AND pl.line_type = 'BASE_SALARY_PRORATED');

INSERT INTO payslip_lines (payslip_id, line_type, line_description, amount, is_deduction)
SELECT p.id, 'MEAL_ALLOWANCE', N'Phụ cấp tiền ăn trưa', ISNULL(c.meal_allowance, 500000), 0
FROM payslips p
JOIN employees e ON p.employee_id = e.id
LEFT JOIN contracts c ON e.id = c.employee_id AND c.is_active = 1
WHERE p.period_id = @PeriodId AND NOT EXISTS (SELECT 1 FROM payslip_lines pl WHERE pl.payslip_id = p.id AND pl.line_type = 'MEAL_ALLOWANCE');

INSERT INTO payslip_lines (payslip_id, line_type, line_description, amount, is_deduction)
SELECT p.id, 'FUEL_ALLOWANCE', N'Phụ cấp xăng xe & đi lại', ISNULL(c.fuel_allowance, 300000), 0
FROM payslips p
JOIN employees e ON p.employee_id = e.id
LEFT JOIN contracts c ON e.id = c.employee_id AND c.is_active = 1
WHERE p.period_id = @PeriodId AND ISNULL(c.fuel_allowance, 0) > 0 AND NOT EXISTS (SELECT 1 FROM payslip_lines pl WHERE pl.payslip_id = p.id AND pl.line_type = 'FUEL_ALLOWANCE');

INSERT INTO payslip_lines (payslip_id, line_type, line_description, amount, is_deduction)
SELECT p.id, 'DEDUCTION_BHXH', N'Trích nộp BHXH (8.0%)', ROUND(ISNULL(c.base_salary, 10000000) * 0.08, 0), 1
FROM payslips p
JOIN employees e ON p.employee_id = e.id
LEFT JOIN contracts c ON e.id = c.employee_id AND c.is_active = 1
WHERE p.period_id = @PeriodId AND NOT EXISTS (SELECT 1 FROM payslip_lines pl WHERE pl.payslip_id = p.id AND pl.line_type = 'DEDUCTION_BHXH');

INSERT INTO payslip_lines (payslip_id, line_type, line_description, amount, is_deduction)
SELECT p.id, 'DEDUCTION_BHYT', N'Trích nộp BHYT (1.5%)', ROUND(ISNULL(c.base_salary, 10000000) * 0.015, 0), 1
FROM payslips p
JOIN employees e ON p.employee_id = e.id
LEFT JOIN contracts c ON e.id = c.employee_id AND c.is_active = 1
WHERE p.period_id = @PeriodId AND NOT EXISTS (SELECT 1 FROM payslip_lines pl WHERE pl.payslip_id = p.id AND pl.line_type = 'DEDUCTION_BHYT');

INSERT INTO payslip_lines (payslip_id, line_type, line_description, amount, is_deduction)
SELECT p.id, 'DEDUCTION_BHTN', N'Trích nộp BHTN (1.0%)', ROUND(ISNULL(c.base_salary, 10000000) * 0.01, 0), 1
FROM payslips p
JOIN employees e ON p.employee_id = e.id
LEFT JOIN contracts c ON e.id = c.employee_id AND c.is_active = 1
WHERE p.period_id = @PeriodId AND NOT EXISTS (SELECT 1 FROM payslip_lines pl WHERE pl.payslip_id = p.id AND pl.line_type = 'DEDUCTION_BHTN');

-- Cập nhật tổng số tiền trong kỳ lương
UPDATE payroll_periods
SET 
    total_gross_amount = (SELECT ISNULL(SUM(gross_salary), 0) FROM payslips WHERE period_id = @PeriodId),
    total_net_amount = (SELECT ISNULL(SUM(net_salary), 0) FROM payslips WHERE period_id = @PeriodId),
    updated_at = GETDATE()
WHERE id = @PeriodId;
GO

-- ============================================================================
-- BƯỚC 6: TỰ ĐỘNG KHỞI TẠO LỊCH PHÂN CA & CÁC STORED PROCEDURES CHUYÊN BIỆT
-- ============================================================================
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
    @PunchAction NVARCHAR(20) = 'AUTO',
    @DeviceInfo NVARCHAR(255) = N'Trạm Camera Điểm Danh Sinh Trắc',
    @PerformedBy NVARCHAR(100) = 'SYSTEM_KIOSK',
    @Confidence DECIMAL(5,2) = 95.0,
    @Distance DECIMAL(6,4) = 0.25,
    @PhotoUrl NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @PunchDateTime IS NULL SET @PunchDateTime = GETDATE();
    DECLARE @PunchTime TIME = CAST(@PunchDateTime AS TIME);

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1 FROM dbo.shift_assignments WITH (UPDLOCK, ROWLOCK)
            WHERE employee_id = @EmployeeId AND shift_id = @ShiftId AND work_date = @WorkDate AND status = 'ACTIVE'
        )
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 
                CAST(0 AS BIT) AS Success,
                'NO_VALID_ASSIGNMENT' AS ResultCode,
                N'Không tìm thấy ca làm việc được phân công.' AS Message,
                NULL AS PunchType, NULL AS PunchTime, NULL AS WorkShiftStatus, 0.0 AS WorkUnits, 0.0 AS ActualHours, NULL AS ShiftName;
            RETURN;
        END

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
                N'Kỳ chấm công của ngày này đã bị khóa sổ bởi Kế toán. Không thể ghi nhận thêm lượt chấm công.' AS Message,
                NULL AS PunchType, NULL AS PunchTime, NULL AS WorkShiftStatus, 0.0 AS WorkUnits, 0.0 AS ActualHours, @ShiftName AS ShiftName;
            RETURN;
        END

        DECLARE @ActualAction NVARCHAR(20) = @PunchAction;
        IF @ActualAction = 'AUTO'
        BEGIN
            IF @CurCheckIn IS NULL
                SET @ActualAction = 'CHECK_IN';
            ELSE IF @CurCheckOut IS NULL
                SET @ActualAction = 'CHECK_OUT';
            ELSE
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT 
                    CAST(0 AS BIT) AS Success,
                    'SHIFT_COMPLETED' AS ResultCode,
                    N'Bạn đã hoàn thành lượt vào và ra cho tất cả các ca được phân công trong ngày.' AS Message,
                    NULL AS PunchType, @CurCheckOut AS PunchTime, @CurStatus AS WorkShiftStatus, 0.0 AS WorkUnits, 0.0 AS ActualHours, @ShiftName AS ShiftName;
                RETURN;
            END
        END

        DECLARE @FinalStatus NVARCHAR(50);
        DECLARE @FinalWorkUnits DECIMAL(3,2) = 0.0;
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
        ELSE
        BEGIN
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

        IF @PhotoUrl IS NOT NULL
        BEGIN
            INSERT INTO dbo.attendance_photos (
                timesheet_id, employee_id, punch_type, photo_url, device_info, captured_at
            )
            VALUES (
                @TimesheetId, @EmployeeId, @ActualAction, @PhotoUrl, @DeviceInfo, @PunchDateTime
            );
        END

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

        SELECT 
            CAST(1 AS BIT) AS Success,
            'SUCCESS' AS ResultCode,
            CONCAT(
                CASE WHEN @ActualAction = 'CHECK_IN' THEN N'Chấm công vào thành công.' ELSE N'Chấm công ra thành công.' END,
                N' Ca: ', @ShiftName, N' lúc ',
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
            N'Không thể ghi nhận chấm công. Vui lòng thử lại.' AS Message,
            NULL AS PunchType, NULL AS PunchTime, NULL AS WorkShiftStatus, 0.0 AS WorkUnits, 0.0 AS ActualHours, NULL AS ShiftName;
    END CATCH
END;
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


-- 7.5. KHỞI TẠO DỮ LIỆU MẪU SINH TRẮC HỌC KHUÔN MẶT (CHẾ ĐỘ TỰ ĐỘNG CHUẨN HÓA 128-D CHO 20 NHÂN VIÊN)
IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 1)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[0.17075, 0.027579, -0.134579, 0.075446, 0.003783, -0.013027, -0.148768, 0.095639, 0.04708, -0.105026, 0.04208, -0.047055, -0.141043, -0.007102, 0.086374, 0.201388, -0.14297, -0.067686, -0.116292, 0.092921, 0.005174, 0.047292, -0.003435, -0.126039, -0.004358, 0.05223, -0.045479, -0.068667, -0.041366, -0.070708, 0.078441, 0.06737, 0.040304, 0.087509, 0.003825, 0.03851, -0.045154, 0.253453, -0.052032, -0.1639, 0.031671, -0.047869, -0.084933, -0.033315, -0.094328, -0.124621, -0.041656, -0.09646, -0.053565, 0.09004, 0.05368, 0.000559, -0.038818, 0.05353, -0.037498, -0.00672, 0.038496, 0.009439, 0.02847, 0.066598, 0.071501, -0.077786, 0.187228, 0.045329, -0.07578, 0.032659, -0.065001, 0.083092, 0.126845, 0.070946, -0.103408, 0.185734, -0.010902, 0.216919, 0.048614, -0.082042, -0.00276, 0.008355, 0.053479, -0.007254, 0.167307, 0.039336, -0.005695, 0.090551, -0.012484, -0.009981, -0.094275, -0.076787, -0.02293, 0.030582, -0.004551, 0.011527, 0.054699, 0.015787, -0.115486, -0.128174, -0.082041, -0.026233, 0.125061, -0.006964, -0.034501, 0.071014, -0.108286, -0.220813, -0.059278, -0.010236, 0.029222, -0.055593, 0.048804, 0.0265, 0.05102, 0.054597, -0.209545, -0.033842, -0.074907, 0.02902, 0.123964, -0.091933, 0.007585, -7.5e-05, -0.162071, 0.040224, 0.084918, 0.144306, 0.174585, -0.079985, -0.04897, -0.035902]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP001] Nguyễn Văn A',
        updated_at = GETDATE()
    WHERE employee_id = 1;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (1, '[0.17075, 0.027579, -0.134579, 0.075446, 0.003783, -0.013027, -0.148768, 0.095639, 0.04708, -0.105026, 0.04208, -0.047055, -0.141043, -0.007102, 0.086374, 0.201388, -0.14297, -0.067686, -0.116292, 0.092921, 0.005174, 0.047292, -0.003435, -0.126039, -0.004358, 0.05223, -0.045479, -0.068667, -0.041366, -0.070708, 0.078441, 0.06737, 0.040304, 0.087509, 0.003825, 0.03851, -0.045154, 0.253453, -0.052032, -0.1639, 0.031671, -0.047869, -0.084933, -0.033315, -0.094328, -0.124621, -0.041656, -0.09646, -0.053565, 0.09004, 0.05368, 0.000559, -0.038818, 0.05353, -0.037498, -0.00672, 0.038496, 0.009439, 0.02847, 0.066598, 0.071501, -0.077786, 0.187228, 0.045329, -0.07578, 0.032659, -0.065001, 0.083092, 0.126845, 0.070946, -0.103408, 0.185734, -0.010902, 0.216919, 0.048614, -0.082042, -0.00276, 0.008355, 0.053479, -0.007254, 0.167307, 0.039336, -0.005695, 0.090551, -0.012484, -0.009981, -0.094275, -0.076787, -0.02293, 0.030582, -0.004551, 0.011527, 0.054699, 0.015787, -0.115486, -0.128174, -0.082041, -0.026233, 0.125061, -0.006964, -0.034501, 0.071014, -0.108286, -0.220813, -0.059278, -0.010236, 0.029222, -0.055593, 0.048804, 0.0265, 0.05102, 0.054597, -0.209545, -0.033842, -0.074907, 0.02902, 0.123964, -0.091933, 0.007585, -7.5e-05, -0.162071, 0.040224, 0.084918, 0.144306, 0.174585, -0.079985, -0.04897, -0.035902]', DATEADD(DAY, -1, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP001] Nguyễn Văn A', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 2)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[0.029153, -0.109346, -0.147806, 0.085686, 0.022046, 0.122706, -0.03485, 0.059017, -0.069337, 0.071433, 0.069861, 0.050615, 0.191444, -0.150771, 0.101203, -0.080566, -0.073715, -0.030504, -0.241061, 0.039995, -0.066196, 0.092143, 0.122046, 0.020176, -0.054132, 0.082232, -0.08694, -0.018023, -0.05578, 0.034293, -0.035431, -0.136999, -0.089439, -0.120671, -0.136601, 0.030585, -0.035676, -0.005081, 0.170057, 0.141106, 0.129666, 0.093438, 0.028275, 0.048042, 0.143394, 0.037996, 0.081604, -0.107889, -0.060349, -0.009441, -0.105144, 0.042623, -0.11303, -0.109828, 0.011585, -0.024441, -0.011967, 0.048182, 0.192025, 0.148784, -0.033182, 0.031097, 0.008797, -0.021393, -0.084878, 0.063497, 0.062685, 0.064755, -0.076084, 0.026241, -0.145067, 0.094171, 0.031026, -0.093117, -0.005281, 0.093078, 0.04025, 0.161199, 0.005322, -0.000501, -0.06961, 0.091793, -0.178989, -0.027811, -0.03858, 0.125934, -0.108095, 0.004059, 0.044917, 0.032285, -0.304068, 0.017317, 0.078008, 0.031757, 0.034487, 0.061196, -0.038197, -0.055455, 0.01663, 0.078495, -0.111586, -0.04921, 0.041163, -0.000567, -0.084246, 0.119253, -0.086101, 0.205246, 0.005159, -0.040235, -0.076338, -0.030195, 0.023734, 0.009134, 0.048078, -0.004679, -0.081879, 0.013021, -0.029511, -0.063202, -0.041942, -0.041663, 0.041896, 0.0708, -0.13065, 0.077591, 0.009943, -0.032097]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP002] Lê Thị Thu Thảo',
        updated_at = GETDATE()
    WHERE employee_id = 2;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (2, '[0.029153, -0.109346, -0.147806, 0.085686, 0.022046, 0.122706, -0.03485, 0.059017, -0.069337, 0.071433, 0.069861, 0.050615, 0.191444, -0.150771, 0.101203, -0.080566, -0.073715, -0.030504, -0.241061, 0.039995, -0.066196, 0.092143, 0.122046, 0.020176, -0.054132, 0.082232, -0.08694, -0.018023, -0.05578, 0.034293, -0.035431, -0.136999, -0.089439, -0.120671, -0.136601, 0.030585, -0.035676, -0.005081, 0.170057, 0.141106, 0.129666, 0.093438, 0.028275, 0.048042, 0.143394, 0.037996, 0.081604, -0.107889, -0.060349, -0.009441, -0.105144, 0.042623, -0.11303, -0.109828, 0.011585, -0.024441, -0.011967, 0.048182, 0.192025, 0.148784, -0.033182, 0.031097, 0.008797, -0.021393, -0.084878, 0.063497, 0.062685, 0.064755, -0.076084, 0.026241, -0.145067, 0.094171, 0.031026, -0.093117, -0.005281, 0.093078, 0.04025, 0.161199, 0.005322, -0.000501, -0.06961, 0.091793, -0.178989, -0.027811, -0.03858, 0.125934, -0.108095, 0.004059, 0.044917, 0.032285, -0.304068, 0.017317, 0.078008, 0.031757, 0.034487, 0.061196, -0.038197, -0.055455, 0.01663, 0.078495, -0.111586, -0.04921, 0.041163, -0.000567, -0.084246, 0.119253, -0.086101, 0.205246, 0.005159, -0.040235, -0.076338, -0.030195, 0.023734, 0.009134, 0.048078, -0.004679, -0.081879, 0.013021, -0.029511, -0.063202, -0.041942, -0.041663, 0.041896, 0.0708, -0.13065, 0.077591, 0.009943, -0.032097]', DATEADD(DAY, -2, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP002] Lê Thị Thu Thảo', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 3)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[0.201679, -0.054352, -0.063057, 0.149072, -0.225558, -0.130941, -0.088025, 0.039467, -0.132895, -0.18959, 0.140223, -0.114876, 0.009511, -0.047587, 0.077356, -0.007722, 0.163104, 0.061153, -0.018906, -0.065263, 0.095226, 0.065615, -0.034873, -0.059838, -0.079373, -0.003128, -0.053611, -0.036456, -0.016856, -0.059988, -0.008089, 0.044471, -0.029779, 0.0091, 0.018582, -0.011623, 0.147176, 0.034229, 0.03847, -0.058828, -0.099934, 0.054911, -0.161161, 0.011487, 0.006998, 0.052664, 0.088966, -0.066295, -0.031622, 0.119368, 0.044036, 0.057559, -0.032057, -0.071398, 0.022702, -0.052286, -0.158656, 0.190325, -0.118998, -0.019954, 0.042682, 0.027563, -0.035845, -0.091011, -0.063699, -0.011492, -0.108262, -0.10432, -0.073277, 0.04499, -0.105855, 0.022963, 0.090724, 0.01835, 0.062472, 0.144237, -0.011418, 0.118163, 0.134156, 0.02215, 0.075497, 0.022656, 0.050612, 0.132324, -0.109344, 0.035615, 0.134988, -0.058185, -0.152594, -0.078366, 0.123075, 0.009279, -0.041762, -0.003935, -0.083073, 0.030121, 0.049364, 0.128031, -0.15381, 0.04674, 0.17006, 0.019787, 0.01033, -0.071969, 0.018899, 0.090626, -0.088117, 0.033954, 0.006429, -0.069997, -0.021934, -0.013779, 0.10414, 0.015657, -0.030745, 0.085795, 0.002618, -0.088212, 0.12167, 0.133168, -0.135108, -0.081364, 0.031367, 0.217044, -0.011927, -0.084358, -0.075896, -0.108524]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP003] Trần Đình Trọng',
        updated_at = GETDATE()
    WHERE employee_id = 3;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (3, '[0.201679, -0.054352, -0.063057, 0.149072, -0.225558, -0.130941, -0.088025, 0.039467, -0.132895, -0.18959, 0.140223, -0.114876, 0.009511, -0.047587, 0.077356, -0.007722, 0.163104, 0.061153, -0.018906, -0.065263, 0.095226, 0.065615, -0.034873, -0.059838, -0.079373, -0.003128, -0.053611, -0.036456, -0.016856, -0.059988, -0.008089, 0.044471, -0.029779, 0.0091, 0.018582, -0.011623, 0.147176, 0.034229, 0.03847, -0.058828, -0.099934, 0.054911, -0.161161, 0.011487, 0.006998, 0.052664, 0.088966, -0.066295, -0.031622, 0.119368, 0.044036, 0.057559, -0.032057, -0.071398, 0.022702, -0.052286, -0.158656, 0.190325, -0.118998, -0.019954, 0.042682, 0.027563, -0.035845, -0.091011, -0.063699, -0.011492, -0.108262, -0.10432, -0.073277, 0.04499, -0.105855, 0.022963, 0.090724, 0.01835, 0.062472, 0.144237, -0.011418, 0.118163, 0.134156, 0.02215, 0.075497, 0.022656, 0.050612, 0.132324, -0.109344, 0.035615, 0.134988, -0.058185, -0.152594, -0.078366, 0.123075, 0.009279, -0.041762, -0.003935, -0.083073, 0.030121, 0.049364, 0.128031, -0.15381, 0.04674, 0.17006, 0.019787, 0.01033, -0.071969, 0.018899, 0.090626, -0.088117, 0.033954, 0.006429, -0.069997, -0.021934, -0.013779, 0.10414, 0.015657, -0.030745, 0.085795, 0.002618, -0.088212, 0.12167, 0.133168, -0.135108, -0.081364, 0.031367, 0.217044, -0.011927, -0.084358, -0.075896, -0.108524]', DATEADD(DAY, -3, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP003] Trần Đình Trọng', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 4)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[-0.024297, 0.070337, -0.056104, 0.031607, 0.041663, -0.131697, 0.011011, 0.016786, -0.024316, -0.046079, -0.020466, -0.227292, 0.048065, -0.02845, 0.099814, 0.068154, 0.117381, -0.004115, -0.040197, 0.096332, -0.0135, -0.029693, -0.023697, -0.13363, -0.04104, 0.0043, 0.188587, -0.048191, -0.025342, -0.108621, 0.065674, -0.15208, 0.05006, 0.032726, 0.140883, -0.01438, 0.052548, 0.03591, -0.097561, 0.037771, -0.168975, -0.131873, 0.019982, 0.029406, -0.036294, 0.136929, -0.073453, 0.089204, 0.014689, -0.120793, -0.052413, 0.212621, 0.055465, -0.039294, 0.032865, 0.083569, -0.048834, 0.007546, 0.024118, 0.083176, 0.037177, -0.048433, -0.008496, -0.001729, -0.007355, 0.140449, 0.042242, -0.016692, -0.015584, 0.056115, -0.085114, -0.06828, 0.109114, -0.065415, 0.141191, -0.115782, -0.068435, -0.023128, -0.026988, -0.187454, -0.061867, 0.011297, -0.140195, -0.181549, 0.026752, -0.084854, -0.073138, 0.066976, 0.025294, -0.189634, -0.054567, -0.023763, 0.053663, 0.099557, 0.077446, 0.029206, -0.096635, 0.140815, -0.097989, 0.087424, 0.20045, 0.012551, -0.069497, -0.013852, -0.076907, -0.144535, -0.054286, -0.047388, -0.057342, 0.002042, 0.104003, -0.075246, 0.141609, 0.021487, 0.115189, 0.086988, 0.233393, 0.119554, -0.117816, -0.01434, 0.06995, 0.033407, -0.076249, 0.12331, 0.082188, -0.050164, -0.004084, 0.017443]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP004] Hoàng Minh Tuấn',
        updated_at = GETDATE()
    WHERE employee_id = 4;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (4, '[-0.024297, 0.070337, -0.056104, 0.031607, 0.041663, -0.131697, 0.011011, 0.016786, -0.024316, -0.046079, -0.020466, -0.227292, 0.048065, -0.02845, 0.099814, 0.068154, 0.117381, -0.004115, -0.040197, 0.096332, -0.0135, -0.029693, -0.023697, -0.13363, -0.04104, 0.0043, 0.188587, -0.048191, -0.025342, -0.108621, 0.065674, -0.15208, 0.05006, 0.032726, 0.140883, -0.01438, 0.052548, 0.03591, -0.097561, 0.037771, -0.168975, -0.131873, 0.019982, 0.029406, -0.036294, 0.136929, -0.073453, 0.089204, 0.014689, -0.120793, -0.052413, 0.212621, 0.055465, -0.039294, 0.032865, 0.083569, -0.048834, 0.007546, 0.024118, 0.083176, 0.037177, -0.048433, -0.008496, -0.001729, -0.007355, 0.140449, 0.042242, -0.016692, -0.015584, 0.056115, -0.085114, -0.06828, 0.109114, -0.065415, 0.141191, -0.115782, -0.068435, -0.023128, -0.026988, -0.187454, -0.061867, 0.011297, -0.140195, -0.181549, 0.026752, -0.084854, -0.073138, 0.066976, 0.025294, -0.189634, -0.054567, -0.023763, 0.053663, 0.099557, 0.077446, 0.029206, -0.096635, 0.140815, -0.097989, 0.087424, 0.20045, 0.012551, -0.069497, -0.013852, -0.076907, -0.144535, -0.054286, -0.047388, -0.057342, 0.002042, 0.104003, -0.075246, 0.141609, 0.021487, 0.115189, 0.086988, 0.233393, 0.119554, -0.117816, -0.01434, 0.06995, 0.033407, -0.076249, 0.12331, 0.082188, -0.050164, -0.004084, 0.017443]', DATEADD(DAY, -4, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP004] Hoàng Minh Tuấn', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 5)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[0.057241, -0.193794, 0.037634, 0.046037, 0.08521, -0.046602, 0.023202, 0.080955, -0.030043, 0.182753, 0.038278, -0.02094, 0.034619, -0.096891, 0.020405, 0.056902, 0.008526, 0.11194, 0.050683, 0.098315, 0.101705, -0.107462, 0.063111, -0.137602, -0.014795, 0.086388, 0.058922, 0.035269, -0.060277, 0.125654, -0.080502, -0.009578, 0.042655, 0.045465, -0.030969, -0.011349, 0.009757, -0.065618, 0.087451, 0.037878, -0.016365, -0.166141, -0.042075, 0.122233, -0.140751, -0.076292, 0.094239, -0.154488, -0.131603, 0.040325, 0.129039, -0.138745, -0.08314, 0.022647, 0.012931, 0.028802, 0.192145, -0.01048, -0.166073, 0.05933, -0.084064, -0.136378, 0.127527, 0.019098, 0.005764, -0.193367, 0.019031, 0.041444, -0.004844, -0.009239, -0.136941, -0.058629, -0.033847, 0.123426, -0.084071, -0.007833, 0.003146, -0.042205, 0.026758, -0.138589, 0.046587, 0.036048, 0.027187, 0.185589, 0.010309, -0.041255, 0.137015, 0.157503, -0.089145, 0.041034, -0.161538, -0.011478, -0.000926, -0.029211, -0.089721, 0.08688, 0.0472, -0.013164, -0.1054, -0.172867, -0.100524, -0.004762, 0.085941, -0.048102, 0.071965, -0.138238, 0.075962, -0.110128, 0.100802, 0.086979, -0.122927, -0.039072, -0.059721, -0.040188, 0.13553, 0.062326, 0.010997, 0.073425, -0.099893, 0.091365, 0.047549, 0.020154, -0.043344, -0.081263, 0.155219, -0.059244, 0.029077, 0.107208]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP005] Phạm Hoàng Nam',
        updated_at = GETDATE()
    WHERE employee_id = 5;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (5, '[0.057241, -0.193794, 0.037634, 0.046037, 0.08521, -0.046602, 0.023202, 0.080955, -0.030043, 0.182753, 0.038278, -0.02094, 0.034619, -0.096891, 0.020405, 0.056902, 0.008526, 0.11194, 0.050683, 0.098315, 0.101705, -0.107462, 0.063111, -0.137602, -0.014795, 0.086388, 0.058922, 0.035269, -0.060277, 0.125654, -0.080502, -0.009578, 0.042655, 0.045465, -0.030969, -0.011349, 0.009757, -0.065618, 0.087451, 0.037878, -0.016365, -0.166141, -0.042075, 0.122233, -0.140751, -0.076292, 0.094239, -0.154488, -0.131603, 0.040325, 0.129039, -0.138745, -0.08314, 0.022647, 0.012931, 0.028802, 0.192145, -0.01048, -0.166073, 0.05933, -0.084064, -0.136378, 0.127527, 0.019098, 0.005764, -0.193367, 0.019031, 0.041444, -0.004844, -0.009239, -0.136941, -0.058629, -0.033847, 0.123426, -0.084071, -0.007833, 0.003146, -0.042205, 0.026758, -0.138589, 0.046587, 0.036048, 0.027187, 0.185589, 0.010309, -0.041255, 0.137015, 0.157503, -0.089145, 0.041034, -0.161538, -0.011478, -0.000926, -0.029211, -0.089721, 0.08688, 0.0472, -0.013164, -0.1054, -0.172867, -0.100524, -0.004762, 0.085941, -0.048102, 0.071965, -0.138238, 0.075962, -0.110128, 0.100802, 0.086979, -0.122927, -0.039072, -0.059721, -0.040188, 0.13553, 0.062326, 0.010997, 0.073425, -0.099893, 0.091365, 0.047549, 0.020154, -0.043344, -0.081263, 0.155219, -0.059244, 0.029077, 0.107208]', DATEADD(DAY, -5, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP005] Phạm Hoàng Nam', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 6)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[-0.054295, -0.017273, 0.11936, -0.034249, -0.056199, -0.065101, -0.072303, -0.09483, 0.041953, 0.118756, 0.225436, -0.060986, -0.028472, 0.038908, 0.079102, -0.001706, 0.128094, 0.103782, 0.121258, -0.046143, 0.035698, 0.149161, -0.092422, -0.044654, 0.047124, 0.087381, -0.085294, 0.133449, -0.04922, -0.160526, -0.140824, 0.023962, 0.096491, 0.10086, 0.071087, -0.028615, -0.005676, -0.011826, -0.17191, -0.107579, -0.174778, 0.076061, 0.031485, 0.064513, -0.008116, -0.018411, -0.000594, -0.186251, -0.088536, 0.00355, -0.055354, -0.048296, 0.138319, -0.102127, -0.082536, -0.084512, 0.180124, -0.056691, -0.150299, -0.035308, -0.056265, 0.048646, -0.015201, -0.070714, 0.030653, -0.06218, 0.056788, -0.089765, -0.139345, 0.019656, 0.107511, 0.10824, -0.042998, -0.028501, 0.143936, 0.073303, -0.015911, -0.080663, -0.114866, -0.053022, -0.087356, 0.04537, -0.037338, 0.04381, 0.066884, 0.045703, -0.095408, -0.090934, -0.071114, -0.124674, 0.040738, 0.077548, -0.083233, 0.063325, 0.038519, 0.146221, 0.02426, 0.012884, -0.023002, -0.037738, -0.013351, -0.058694, 0.052796, 0.161857, 0.14086, 0.007032, 0.027241, 0.114725, 0.107321, 0.128021, 0.0151, -0.031835, 0.048033, -0.099128, 0.000704, -0.162972, -0.056308, -0.137502, 0.168992, 0.139823, 0.039675, -0.038018, -0.127281, -0.006994, -0.01353, 0.092068, -0.025246, 0.058844]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP006] Đỗ Thị Mai Linh',
        updated_at = GETDATE()
    WHERE employee_id = 6;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (6, '[-0.054295, -0.017273, 0.11936, -0.034249, -0.056199, -0.065101, -0.072303, -0.09483, 0.041953, 0.118756, 0.225436, -0.060986, -0.028472, 0.038908, 0.079102, -0.001706, 0.128094, 0.103782, 0.121258, -0.046143, 0.035698, 0.149161, -0.092422, -0.044654, 0.047124, 0.087381, -0.085294, 0.133449, -0.04922, -0.160526, -0.140824, 0.023962, 0.096491, 0.10086, 0.071087, -0.028615, -0.005676, -0.011826, -0.17191, -0.107579, -0.174778, 0.076061, 0.031485, 0.064513, -0.008116, -0.018411, -0.000594, -0.186251, -0.088536, 0.00355, -0.055354, -0.048296, 0.138319, -0.102127, -0.082536, -0.084512, 0.180124, -0.056691, -0.150299, -0.035308, -0.056265, 0.048646, -0.015201, -0.070714, 0.030653, -0.06218, 0.056788, -0.089765, -0.139345, 0.019656, 0.107511, 0.10824, -0.042998, -0.028501, 0.143936, 0.073303, -0.015911, -0.080663, -0.114866, -0.053022, -0.087356, 0.04537, -0.037338, 0.04381, 0.066884, 0.045703, -0.095408, -0.090934, -0.071114, -0.124674, 0.040738, 0.077548, -0.083233, 0.063325, 0.038519, 0.146221, 0.02426, 0.012884, -0.023002, -0.037738, -0.013351, -0.058694, 0.052796, 0.161857, 0.14086, 0.007032, 0.027241, 0.114725, 0.107321, 0.128021, 0.0151, -0.031835, 0.048033, -0.099128, 0.000704, -0.162972, -0.056308, -0.137502, 0.168992, 0.139823, 0.039675, -0.038018, -0.127281, -0.006994, -0.01353, 0.092068, -0.025246, 0.058844]', DATEADD(DAY, -6, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP006] Đỗ Thị Mai Linh', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 7)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[-0.043067, -0.032185, 0.038097, 0.011776, -0.109884, 0.084723, 0.094792, -0.044732, 0.079825, 0.008623, 0.080521, -0.07958, -0.086438, -0.115017, -0.017169, -0.017783, -0.034576, -0.141462, 0.163068, 0.037169, 0.033352, -0.069141, 0.187285, -0.00537, -0.015765, -0.075632, 0.11256, 0.058808, -0.016984, -0.042635, -0.121411, 0.05473, -0.090794, -0.003075, 0.07327, 0.004324, 0.135435, -0.027924, 0.075245, -0.035739, -0.059766, -0.183679, -0.010571, -0.123136, -0.073563, 0.066017, 0.027851, -0.07204, -0.04463, -0.069995, -0.101205, 0.154753, 0.098558, -0.133999, -0.078683, 0.05578, -0.010615, -0.032719, -0.005854, 0.049209, -0.049078, -0.094618, 0.074652, -0.045154, 0.142535, -0.196157, 0.005045, 0.033524, 0.045778, 0.029573, -0.041569, -0.095973, 0.12053, 0.042232, 0.033369, 0.020059, -0.020674, -0.042178, -0.002966, -0.261758, 0.009115, 0.139408, -0.15708, 0.096689, -0.036014, 0.094407, 0.118369, 0.079701, 0.051767, -0.117721, 0.048846, 0.111797, -0.112344, 0.055111, 0.029611, -0.054546, 0.085548, 0.011376, 0.216638, -0.079401, 0.046837, -0.003276, 0.039165, 0.125494, 0.013287, -0.051807, -0.106638, -0.082975, 0.070467, -0.093532, 0.054479, 0.000705, -0.111798, -0.103047, -0.084433, -0.056369, 0.121965, 0.038428, -0.151372, -0.15457, 0.081833, 0.107393, 0.14495, 0.01729, 0.143604, 0.004259, 0.099363, 0.042897]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP007] Vũ Quốc Anh',
        updated_at = GETDATE()
    WHERE employee_id = 7;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (7, '[-0.043067, -0.032185, 0.038097, 0.011776, -0.109884, 0.084723, 0.094792, -0.044732, 0.079825, 0.008623, 0.080521, -0.07958, -0.086438, -0.115017, -0.017169, -0.017783, -0.034576, -0.141462, 0.163068, 0.037169, 0.033352, -0.069141, 0.187285, -0.00537, -0.015765, -0.075632, 0.11256, 0.058808, -0.016984, -0.042635, -0.121411, 0.05473, -0.090794, -0.003075, 0.07327, 0.004324, 0.135435, -0.027924, 0.075245, -0.035739, -0.059766, -0.183679, -0.010571, -0.123136, -0.073563, 0.066017, 0.027851, -0.07204, -0.04463, -0.069995, -0.101205, 0.154753, 0.098558, -0.133999, -0.078683, 0.05578, -0.010615, -0.032719, -0.005854, 0.049209, -0.049078, -0.094618, 0.074652, -0.045154, 0.142535, -0.196157, 0.005045, 0.033524, 0.045778, 0.029573, -0.041569, -0.095973, 0.12053, 0.042232, 0.033369, 0.020059, -0.020674, -0.042178, -0.002966, -0.261758, 0.009115, 0.139408, -0.15708, 0.096689, -0.036014, 0.094407, 0.118369, 0.079701, 0.051767, -0.117721, 0.048846, 0.111797, -0.112344, 0.055111, 0.029611, -0.054546, 0.085548, 0.011376, 0.216638, -0.079401, 0.046837, -0.003276, 0.039165, 0.125494, 0.013287, -0.051807, -0.106638, -0.082975, 0.070467, -0.093532, 0.054479, 0.000705, -0.111798, -0.103047, -0.084433, -0.056369, 0.121965, 0.038428, -0.151372, -0.15457, 0.081833, 0.107393, 0.14495, 0.01729, 0.143604, 0.004259, 0.099363, 0.042897]', DATEADD(DAY, -7, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP007] Vũ Quốc Anh', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 8)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[0.054145, 0.069337, -0.174259, -0.020862, -0.18846, -0.075294, 0.007904, 0.116265, -0.021752, -0.040955, 0.037569, 0.002472, 0.017938, 0.024789, 0.054562, 0.108244, -0.107696, 0.047441, 0.002553, -0.048798, 0.069783, -0.039193, 0.11217, -0.094666, -0.096006, -0.022616, -0.010521, 0.100396, 0.070507, 0.10909, -0.080997, 0.040622, -0.032884, -0.08901, -0.102571, 0.20266, 0.100131, -0.129403, 0.028165, -0.085429, 0.081503, -0.003635, -0.126997, 0.018832, 0.114448, -0.085595, 0.073637, 0.068944, 0.027159, -0.220259, 0.108209, -0.089925, 0.041698, 0.024031, 0.085753, 0.012363, -0.074071, 0.079513, -0.067689, -0.01733, -0.008414, 0.012192, -0.114055, -0.023586, 0.152728, -0.177216, 0.137375, -0.041825, -0.042723, 0.015919, -0.017708, 0.007499, 0.022552, -0.139531, 0.024001, -0.063813, 0.060709, 0.072015, 0.067977, -0.023932, -0.112046, 0.0686, -0.091257, 0.043902, -0.024813, 0.147284, -0.069072, -0.071661, 0.055431, 0.034679, -0.103473, -0.033966, -0.050249, -0.017867, 0.049612, 0.121005, 0.050271, 0.13154, -0.133133, -0.153257, 0.013201, 0.023947, 0.026728, 0.156526, 0.007336, 0.211294, 0.058423, -0.088912, 0.07391, -0.004937, -0.051288, -0.004719, 0.058383, 0.199069, 0.110921, -0.080603, -0.063185, -0.171009, 0.106705, -0.103925, 0.084222, 0.068103, 0.009025, 0.005034, -0.089727, -0.043883, -0.052702, -0.185591]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP008] Bùi Thị Bích Ngọc',
        updated_at = GETDATE()
    WHERE employee_id = 8;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (8, '[0.054145, 0.069337, -0.174259, -0.020862, -0.18846, -0.075294, 0.007904, 0.116265, -0.021752, -0.040955, 0.037569, 0.002472, 0.017938, 0.024789, 0.054562, 0.108244, -0.107696, 0.047441, 0.002553, -0.048798, 0.069783, -0.039193, 0.11217, -0.094666, -0.096006, -0.022616, -0.010521, 0.100396, 0.070507, 0.10909, -0.080997, 0.040622, -0.032884, -0.08901, -0.102571, 0.20266, 0.100131, -0.129403, 0.028165, -0.085429, 0.081503, -0.003635, -0.126997, 0.018832, 0.114448, -0.085595, 0.073637, 0.068944, 0.027159, -0.220259, 0.108209, -0.089925, 0.041698, 0.024031, 0.085753, 0.012363, -0.074071, 0.079513, -0.067689, -0.01733, -0.008414, 0.012192, -0.114055, -0.023586, 0.152728, -0.177216, 0.137375, -0.041825, -0.042723, 0.015919, -0.017708, 0.007499, 0.022552, -0.139531, 0.024001, -0.063813, 0.060709, 0.072015, 0.067977, -0.023932, -0.112046, 0.0686, -0.091257, 0.043902, -0.024813, 0.147284, -0.069072, -0.071661, 0.055431, 0.034679, -0.103473, -0.033966, -0.050249, -0.017867, 0.049612, 0.121005, 0.050271, 0.13154, -0.133133, -0.153257, 0.013201, 0.023947, 0.026728, 0.156526, 0.007336, 0.211294, 0.058423, -0.088912, 0.07391, -0.004937, -0.051288, -0.004719, 0.058383, 0.199069, 0.110921, -0.080603, -0.063185, -0.171009, 0.106705, -0.103925, 0.084222, 0.068103, 0.009025, 0.005034, -0.089727, -0.043883, -0.052702, -0.185591]', DATEADD(DAY, -8, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP008] Bùi Thị Bích Ngọc', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 9)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[-0.208611, 0.125083, 0.006767, -0.0079, 0.001074, 0.084047, 0.092915, 0.024076, 0.065982, -0.089204, 0.076588, -0.118009, -0.078815, 0.085008, -0.134749, -0.179535, -0.023323, 0.023893, -0.028172, -0.090754, -0.139588, 0.085191, 0.00051, 0.129974, -0.042186, -0.008177, -0.013871, 0.050254, 0.079718, -0.170795, -0.060205, -0.098127, -0.022731, -0.065431, 0.026534, 0.02801, 0.044397, -0.007849, -0.029314, 0.06708, 0.054899, -0.164505, -0.046627, 0.132087, 0.097764, -0.032447, 0.114288, 0.123994, 0.203308, -0.002556, 0.102295, -0.018466, -0.043547, -0.040403, -0.118428, 0.011216, 0.090718, 0.126843, -0.021331, -0.115272, -0.094081, -0.005864, 0.069782, -0.085183, 0.125985, 0.086943, 0.151677, 0.017767, 0.167797, -0.141207, -0.12893, -0.102035, 0.133814, -0.144781, -0.00312, 0.053464, 0.089769, 0.072402, 0.076088, -0.122508, 0.009468, -0.110445, 0.058859, 0.089492, -0.180017, -0.056264, -0.051571, 0.165956, 0.13081, 0.078186, -0.078854, -0.129839, -0.01525, -0.089034, -0.126962, 0.006159, 0.09613, -0.0016, 0.008635, 0.037517, -0.007036, 0.119765, 0.017462, 0.122264, 0.019006, -0.080711, 0.005553, -0.03781, -0.022259, 0.012026, -0.023108, -0.064574, -0.050694, 0.050193, 0.092114, -0.119748, -0.003183, -0.01848, -0.036992, -0.024988, -0.03365, 0.136792, 0.011251, 0.056664, -0.046303, 0.095049, -0.096793, -0.06455]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP009] Đinh Quang Huy',
        updated_at = GETDATE()
    WHERE employee_id = 9;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (9, '[-0.208611, 0.125083, 0.006767, -0.0079, 0.001074, 0.084047, 0.092915, 0.024076, 0.065982, -0.089204, 0.076588, -0.118009, -0.078815, 0.085008, -0.134749, -0.179535, -0.023323, 0.023893, -0.028172, -0.090754, -0.139588, 0.085191, 0.00051, 0.129974, -0.042186, -0.008177, -0.013871, 0.050254, 0.079718, -0.170795, -0.060205, -0.098127, -0.022731, -0.065431, 0.026534, 0.02801, 0.044397, -0.007849, -0.029314, 0.06708, 0.054899, -0.164505, -0.046627, 0.132087, 0.097764, -0.032447, 0.114288, 0.123994, 0.203308, -0.002556, 0.102295, -0.018466, -0.043547, -0.040403, -0.118428, 0.011216, 0.090718, 0.126843, -0.021331, -0.115272, -0.094081, -0.005864, 0.069782, -0.085183, 0.125985, 0.086943, 0.151677, 0.017767, 0.167797, -0.141207, -0.12893, -0.102035, 0.133814, -0.144781, -0.00312, 0.053464, 0.089769, 0.072402, 0.076088, -0.122508, 0.009468, -0.110445, 0.058859, 0.089492, -0.180017, -0.056264, -0.051571, 0.165956, 0.13081, 0.078186, -0.078854, -0.129839, -0.01525, -0.089034, -0.126962, 0.006159, 0.09613, -0.0016, 0.008635, 0.037517, -0.007036, 0.119765, 0.017462, 0.122264, 0.019006, -0.080711, 0.005553, -0.03781, -0.022259, 0.012026, -0.023108, -0.064574, -0.050694, 0.050193, 0.092114, -0.119748, -0.003183, -0.01848, -0.036992, -0.024988, -0.03365, 0.136792, 0.011251, 0.056664, -0.046303, 0.095049, -0.096793, -0.06455]', DATEADD(DAY, -9, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP009] Đinh Quang Huy', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 10)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[-0.116209, 0.019199, -0.103216, 0.021169, -0.136359, -0.004073, -0.015796, 0.117363, 0.24346, 0.02755, 0.148718, 0.013403, 0.013928, 0.030542, 0.029627, 0.118321, -0.044763, 0.082622, -0.00752, -0.004192, -0.039592, 0.051246, 0.044248, 0.050999, -0.020399, -0.056506, 0.102185, -0.016151, -0.049942, -0.002032, -0.109333, 0.102657, -0.001797, -0.084749, -0.067739, 0.109199, -0.010029, 0.046071, -0.084096, 0.211794, 0.089135, 0.145666, -0.004995, -0.018764, -0.035797, -0.178707, 0.185276, -0.014088, 0.015052, 0.076609, -0.17535, -0.037155, 0.09031, 0.051126, -0.023238, -0.176571, 0.012414, 0.11881, 0.016198, -0.158776, 0.003953, 0.00044, 0.036543, 0.077311, -0.183616, -0.060649, 0.059005, -0.115599, -0.176053, 0.060871, -0.06905, 0.187338, 0.109848, -0.010623, -0.039913, -0.03205, -0.049829, -0.054656, 0.098498, -0.03413, -0.010288, -0.060809, 0.03245, 0.036216, 0.068364, -0.037123, 0.095668, 0.147003, -0.000443, -0.04067, -0.061812, -0.053306, -0.087609, -0.018294, -0.103713, 0.081078, 0.024859, 0.080091, 0.086253, 0.150121, 0.122474, -0.133717, -0.116235, 0.029553, -0.07742, 0.011918, 0.068429, 0.059009, 0.008746, 0.058545, 0.058539, -0.104943, 0.033399, -0.043006, 0.002384, -0.161756, -0.068306, -0.085074, -0.075587, -0.050057, -0.186555, 0.108276, -0.158407, -0.07348, 0.00491, 0.06184, 0.01238, -0.028502]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP010] Nguyễn Hải Yến',
        updated_at = GETDATE()
    WHERE employee_id = 10;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (10, '[-0.116209, 0.019199, -0.103216, 0.021169, -0.136359, -0.004073, -0.015796, 0.117363, 0.24346, 0.02755, 0.148718, 0.013403, 0.013928, 0.030542, 0.029627, 0.118321, -0.044763, 0.082622, -0.00752, -0.004192, -0.039592, 0.051246, 0.044248, 0.050999, -0.020399, -0.056506, 0.102185, -0.016151, -0.049942, -0.002032, -0.109333, 0.102657, -0.001797, -0.084749, -0.067739, 0.109199, -0.010029, 0.046071, -0.084096, 0.211794, 0.089135, 0.145666, -0.004995, -0.018764, -0.035797, -0.178707, 0.185276, -0.014088, 0.015052, 0.076609, -0.17535, -0.037155, 0.09031, 0.051126, -0.023238, -0.176571, 0.012414, 0.11881, 0.016198, -0.158776, 0.003953, 0.00044, 0.036543, 0.077311, -0.183616, -0.060649, 0.059005, -0.115599, -0.176053, 0.060871, -0.06905, 0.187338, 0.109848, -0.010623, -0.039913, -0.03205, -0.049829, -0.054656, 0.098498, -0.03413, -0.010288, -0.060809, 0.03245, 0.036216, 0.068364, -0.037123, 0.095668, 0.147003, -0.000443, -0.04067, -0.061812, -0.053306, -0.087609, -0.018294, -0.103713, 0.081078, 0.024859, 0.080091, 0.086253, 0.150121, 0.122474, -0.133717, -0.116235, 0.029553, -0.07742, 0.011918, 0.068429, 0.059009, 0.008746, 0.058545, 0.058539, -0.104943, 0.033399, -0.043006, 0.002384, -0.161756, -0.068306, -0.085074, -0.075587, -0.050057, -0.186555, 0.108276, -0.158407, -0.07348, 0.00491, 0.06184, 0.01238, -0.028502]', DATEADD(DAY, -10, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP010] Nguyễn Hải Yến', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 11)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[-0.029664, 0.056307, -0.031031, 0.023777, -0.059901, 0.171702, -0.025556, 0.056588, 0.020261, 0.070552, -0.00927, 0.193825, 0.03203, 0.076653, -0.09354, -0.021652, -0.049266, -0.0196, 0.008721, 0.128268, -0.060589, -0.023893, 0.007566, 0.147375, -0.049344, -0.038667, -0.107218, -0.063744, -0.09438, -0.069585, 0.101338, -0.040678, 0.222053, -0.093836, 0.084156, 0.22168, 0.018036, 0.008962, -0.013635, -0.00626, -0.097447, 0.089721, -0.098673, 0.076156, -0.002902, 0.031942, 0.030278, -0.02217, -0.01928, 0.107905, 0.164947, -0.00224, -0.044496, -0.035436, 0.159239, -0.091867, -0.033436, 0.078015, 0.120967, -0.029237, -0.025744, -0.034391, -0.047648, 0.059709, 0.025913, -0.013478, -0.02628, -0.103378, 0.083353, 0.021291, 0.017728, 0.101507, -0.147266, 0.00455, -0.19445, 0.080364, -0.036576, -0.17776, -0.003414, 0.094038, 0.170114, 0.018838, 0.003791, 0.018986, -0.145207, 0.009195, -0.049887, 0.036906, -0.244233, -0.080377, -0.074268, -0.015195, -0.065851, -0.115266, -0.118341, -0.068398, -0.1463, 0.129208, -0.102038, 0.018862, -0.158443, 0.034315, 0.047514, -0.020558, -0.076139, 0.015566, -0.046129, -0.002558, 0.169558, 0.127295, -0.033088, -0.083194, 0.049203, -0.00773, 0.122553, 0.085791, 0.07549, 0.138334, 0.033501, 0.108604, 0.026743, 0.010304, -0.094879, -0.080203, -0.024141, -0.099671, -0.105577, -0.101592]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP011] Phạm Minh Đức',
        updated_at = GETDATE()
    WHERE employee_id = 11;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (11, '[-0.029664, 0.056307, -0.031031, 0.023777, -0.059901, 0.171702, -0.025556, 0.056588, 0.020261, 0.070552, -0.00927, 0.193825, 0.03203, 0.076653, -0.09354, -0.021652, -0.049266, -0.0196, 0.008721, 0.128268, -0.060589, -0.023893, 0.007566, 0.147375, -0.049344, -0.038667, -0.107218, -0.063744, -0.09438, -0.069585, 0.101338, -0.040678, 0.222053, -0.093836, 0.084156, 0.22168, 0.018036, 0.008962, -0.013635, -0.00626, -0.097447, 0.089721, -0.098673, 0.076156, -0.002902, 0.031942, 0.030278, -0.02217, -0.01928, 0.107905, 0.164947, -0.00224, -0.044496, -0.035436, 0.159239, -0.091867, -0.033436, 0.078015, 0.120967, -0.029237, -0.025744, -0.034391, -0.047648, 0.059709, 0.025913, -0.013478, -0.02628, -0.103378, 0.083353, 0.021291, 0.017728, 0.101507, -0.147266, 0.00455, -0.19445, 0.080364, -0.036576, -0.17776, -0.003414, 0.094038, 0.170114, 0.018838, 0.003791, 0.018986, -0.145207, 0.009195, -0.049887, 0.036906, -0.244233, -0.080377, -0.074268, -0.015195, -0.065851, -0.115266, -0.118341, -0.068398, -0.1463, 0.129208, -0.102038, 0.018862, -0.158443, 0.034315, 0.047514, -0.020558, -0.076139, 0.015566, -0.046129, -0.002558, 0.169558, 0.127295, -0.033088, -0.083194, 0.049203, -0.00773, 0.122553, 0.085791, 0.07549, 0.138334, 0.033501, 0.108604, 0.026743, 0.010304, -0.094879, -0.080203, -0.024141, -0.099671, -0.105577, -0.101592]', DATEADD(DAY, -11, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP011] Phạm Minh Đức', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 12)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[-0.092647, 0.098664, -0.008991, -0.003043, 0.189379, 0.02811, -0.096427, -0.052947, 0.044958, 0.012796, -0.005537, -0.098093, 0.091426, 0.005618, 0.020387, -0.049099, 0.077504, 0.067169, -0.00166, 0.086561, -0.138741, 0.049508, 0.150468, -0.09021, -0.028633, -0.159089, -0.023044, -0.081119, -0.104146, 0.092608, 0.037939, -0.012934, -0.015959, 0.006632, -0.042554, 0.073861, -0.053967, 0.016169, 0.01677, 0.024047, 0.115078, 0.004906, 0.025247, -0.000293, 0.01649, -0.058974, 0.190451, -0.095873, 0.095825, -0.070115, -0.028902, -0.131206, -0.111995, -0.146199, -0.043468, 0.127743, -0.065017, 0.074735, 0.018953, -0.086043, 0.086417, 0.145727, 0.008467, -0.021118, 0.05512, -0.260905, 0.051255, -0.006223, 0.008888, -0.052908, -0.031129, -0.113875, 0.001873, 0.066594, -0.115094, 0.138353, 0.007623, -0.145427, -0.046006, -0.137527, -0.047011, 0.050701, -0.077373, 0.048097, 0.005085, 0.054688, 0.109247, 0.094977, -0.145224, 0.17617, 0.070454, 0.013112, 0.055653, 0.273758, 0.035231, 0.012197, -0.141171, 0.027914, 0.032749, 0.123732, -0.054756, -0.047113, -0.106915, 0.04085, 0.015959, 0.036887, -0.158204, 0.058441, 0.003005, -0.161157, 0.023146, 0.190055, 0.021556, 0.066328, 0.020767, -0.09033, -0.01433, 0.031986, -0.180673, -0.037633, -0.036795, -0.061002, 0.093482, 0.000179, 0.065893, 0.067613, 0.052701, -0.11611]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP012] Lương Kim Ngân',
        updated_at = GETDATE()
    WHERE employee_id = 12;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (12, '[-0.092647, 0.098664, -0.008991, -0.003043, 0.189379, 0.02811, -0.096427, -0.052947, 0.044958, 0.012796, -0.005537, -0.098093, 0.091426, 0.005618, 0.020387, -0.049099, 0.077504, 0.067169, -0.00166, 0.086561, -0.138741, 0.049508, 0.150468, -0.09021, -0.028633, -0.159089, -0.023044, -0.081119, -0.104146, 0.092608, 0.037939, -0.012934, -0.015959, 0.006632, -0.042554, 0.073861, -0.053967, 0.016169, 0.01677, 0.024047, 0.115078, 0.004906, 0.025247, -0.000293, 0.01649, -0.058974, 0.190451, -0.095873, 0.095825, -0.070115, -0.028902, -0.131206, -0.111995, -0.146199, -0.043468, 0.127743, -0.065017, 0.074735, 0.018953, -0.086043, 0.086417, 0.145727, 0.008467, -0.021118, 0.05512, -0.260905, 0.051255, -0.006223, 0.008888, -0.052908, -0.031129, -0.113875, 0.001873, 0.066594, -0.115094, 0.138353, 0.007623, -0.145427, -0.046006, -0.137527, -0.047011, 0.050701, -0.077373, 0.048097, 0.005085, 0.054688, 0.109247, 0.094977, -0.145224, 0.17617, 0.070454, 0.013112, 0.055653, 0.273758, 0.035231, 0.012197, -0.141171, 0.027914, 0.032749, 0.123732, -0.054756, -0.047113, -0.106915, 0.04085, 0.015959, 0.036887, -0.158204, 0.058441, 0.003005, -0.161157, 0.023146, 0.190055, 0.021556, 0.066328, 0.020767, -0.09033, -0.01433, 0.031986, -0.180673, -0.037633, -0.036795, -0.061002, 0.093482, 0.000179, 0.065893, 0.067613, 0.052701, -0.11611]', DATEADD(DAY, -12, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP012] Lương Kim Ngân', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 13)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[0.046569, -0.10077, 0.030475, 0.051029, 0.000892, -0.067717, 0.030084, 0.096677, -0.171438, -0.105065, -0.166017, 0.080998, 0.150983, 9.4e-05, 0.046233, -0.041984, -0.090714, 0.007652, -0.016524, 0.073804, -0.022717, 0.061736, -0.035876, 0.090017, 0.141645, 0.030823, 0.064264, -0.055622, -0.078004, -0.127427, -0.000914, -0.04963, -0.152609, 0.11601, 0.037788, -0.009602, 0.006693, 0.035872, 0.001991, -0.163615, -0.032426, 0.090824, -0.103046, 0.048047, 0.064478, -0.139177, 0.047262, -0.075132, 0.009364, 0.086908, -0.086868, 0.10365, -0.036747, 0.037792, 0.024872, -0.061869, -0.04908, 0.234322, -0.223668, 0.005092, -0.105718, 0.098838, 0.12864, 0.011822, 0.14219, 0.01648, 0.037738, 0.090318, -0.093013, 0.003547, -0.036987, -0.119202, 0.002795, -0.004412, -0.015079, 0.005315, 0.079844, 0.168965, -0.035086, -0.031436, -0.049319, -0.184405, -0.078879, -0.141439, -0.03332, 0.100716, -0.030345, 0.1039, 0.075769, 0.003546, 0.056729, 0.043634, 0.101733, 0.062457, -0.006563, -0.129148, -0.141586, -0.032603, -0.013804, 0.088016, -0.064345, 0.083082, 0.195629, 0.010227, 0.041697, 0.060091, 0.062761, 0.09799, -0.082251, -0.116397, 0.036388, 0.099411, -0.013562, -0.143303, 0.045677, -0.00326, 0.113004, 0.055736, -0.172331, 0.103963, -0.016437, 0.107419, 0.050765, -0.080914, -0.083984, -0.172241, 0.016501, -0.062643]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP013] Tạ Văn Kiên',
        updated_at = GETDATE()
    WHERE employee_id = 13;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (13, '[0.046569, -0.10077, 0.030475, 0.051029, 0.000892, -0.067717, 0.030084, 0.096677, -0.171438, -0.105065, -0.166017, 0.080998, 0.150983, 9.4e-05, 0.046233, -0.041984, -0.090714, 0.007652, -0.016524, 0.073804, -0.022717, 0.061736, -0.035876, 0.090017, 0.141645, 0.030823, 0.064264, -0.055622, -0.078004, -0.127427, -0.000914, -0.04963, -0.152609, 0.11601, 0.037788, -0.009602, 0.006693, 0.035872, 0.001991, -0.163615, -0.032426, 0.090824, -0.103046, 0.048047, 0.064478, -0.139177, 0.047262, -0.075132, 0.009364, 0.086908, -0.086868, 0.10365, -0.036747, 0.037792, 0.024872, -0.061869, -0.04908, 0.234322, -0.223668, 0.005092, -0.105718, 0.098838, 0.12864, 0.011822, 0.14219, 0.01648, 0.037738, 0.090318, -0.093013, 0.003547, -0.036987, -0.119202, 0.002795, -0.004412, -0.015079, 0.005315, 0.079844, 0.168965, -0.035086, -0.031436, -0.049319, -0.184405, -0.078879, -0.141439, -0.03332, 0.100716, -0.030345, 0.1039, 0.075769, 0.003546, 0.056729, 0.043634, 0.101733, 0.062457, -0.006563, -0.129148, -0.141586, -0.032603, -0.013804, 0.088016, -0.064345, 0.083082, 0.195629, 0.010227, 0.041697, 0.060091, 0.062761, 0.09799, -0.082251, -0.116397, 0.036388, 0.099411, -0.013562, -0.143303, 0.045677, -0.00326, 0.113004, 0.055736, -0.172331, 0.103963, -0.016437, 0.107419, 0.050765, -0.080914, -0.083984, -0.172241, 0.016501, -0.062643]', DATEADD(DAY, -13, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP013] Tạ Văn Kiên', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 14)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[-0.142909, 0.036512, 0.044293, -0.120814, -0.114568, -0.146073, -0.039285, 0.066038, 0.110429, -0.26053, -0.02612, 0.152033, -0.073112, -0.080893, -0.023234, -0.12736, -0.048245, -0.025374, 0.050789, -0.002097, -0.070958, 0.099878, -0.104851, 0.018931, -0.007028, -0.207207, 0.040964, -0.03783, 0.006403, -0.123173, -0.029013, 0.038574, 0.011415, -0.197421, -0.01048, -0.056313, 0.048316, -0.018916, 0.126556, -0.020849, 0.021004, -0.05084, 0.044388, 0.005126, -0.041907, 0.037321, -0.022795, 0.104566, 0.110371, -0.074365, 0.021613, 0.14808, -0.038199, -0.079182, 0.029634, 0.018069, 0.086605, 0.159007, -0.038098, 0.063525, 0.037785, 0.010366, 0.122174, -0.064256, -0.013324, 0.123439, -0.145892, -0.029069, -0.036697, -0.005726, -0.015045, 0.063612, -0.033796, -0.103879, 0.098713, -0.082267, -0.066561, 0.144724, -0.119476, 0.081185, -0.024487, -0.094609, 0.130558, -0.032956, 0.003266, 0.147714, -0.062703, 0.175321, -0.128464, -0.001796, 0.194415, 0.037034, 0.032042, 0.113648, -0.137757, -0.018833, 0.12162, 0.078245, -0.028602, 0.055101, -0.036336, -0.065693, 0.015193, -0.061108, 0.049341, 0.044219, 0.13855, -0.041035, -0.082706, 0.074075, 0.035972, 0.162515, -0.162907, 0.125566, 0.057889, 0.0893, 0.000278, -0.024623, -0.012347, 0.167291, -0.051967, -0.025133, 0.004505, -0.12808, -0.016662, -0.039995, 0.077638, -0.058913]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP014] Hà Phương Thảo',
        updated_at = GETDATE()
    WHERE employee_id = 14;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (14, '[-0.142909, 0.036512, 0.044293, -0.120814, -0.114568, -0.146073, -0.039285, 0.066038, 0.110429, -0.26053, -0.02612, 0.152033, -0.073112, -0.080893, -0.023234, -0.12736, -0.048245, -0.025374, 0.050789, -0.002097, -0.070958, 0.099878, -0.104851, 0.018931, -0.007028, -0.207207, 0.040964, -0.03783, 0.006403, -0.123173, -0.029013, 0.038574, 0.011415, -0.197421, -0.01048, -0.056313, 0.048316, -0.018916, 0.126556, -0.020849, 0.021004, -0.05084, 0.044388, 0.005126, -0.041907, 0.037321, -0.022795, 0.104566, 0.110371, -0.074365, 0.021613, 0.14808, -0.038199, -0.079182, 0.029634, 0.018069, 0.086605, 0.159007, -0.038098, 0.063525, 0.037785, 0.010366, 0.122174, -0.064256, -0.013324, 0.123439, -0.145892, -0.029069, -0.036697, -0.005726, -0.015045, 0.063612, -0.033796, -0.103879, 0.098713, -0.082267, -0.066561, 0.144724, -0.119476, 0.081185, -0.024487, -0.094609, 0.130558, -0.032956, 0.003266, 0.147714, -0.062703, 0.175321, -0.128464, -0.001796, 0.194415, 0.037034, 0.032042, 0.113648, -0.137757, -0.018833, 0.12162, 0.078245, -0.028602, 0.055101, -0.036336, -0.065693, 0.015193, -0.061108, 0.049341, 0.044219, 0.13855, -0.041035, -0.082706, 0.074075, 0.035972, 0.162515, -0.162907, 0.125566, 0.057889, 0.0893, 0.000278, -0.024623, -0.012347, 0.167291, -0.051967, -0.025133, 0.004505, -0.12808, -0.016662, -0.039995, 0.077638, -0.058913]', DATEADD(DAY, -14, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP014] Hà Phương Thảo', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 15)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[0.09648, 0.028163, 0.071946, -0.026625, 0.059144, 0.128953, -0.089607, 0.00145, 0.034836, -0.110269, -0.079102, 0.120304, 0.19397, -0.165935, 0.019495, -0.032195, -0.100986, 0.030843, 0.172455, 0.040879, 0.074417, -0.081567, 0.012653, -0.01073, -0.130615, 0.002495, 0.064056, -0.079508, 0.083264, -0.115528, 0.06966, 0.085335, 0.049031, 0.115325, 0.035937, 0.061971, -0.020636, -0.029946, 0.038895, -0.074296, -0.340161, -0.095368, -0.146192, 0.00664, -0.045507, 0.006419, 0.053595, 0.059736, 0.013086, 0.055428, 0.042462, -0.034244, 0.067752, 0.017009, 0.14842, 0.139574, 0.075121, -0.215893, -0.006701, -0.04887, 0.096324, 0.031076, -0.123108, 0.212288, -0.108246, 0.014986, 0.038932, -0.081511, 0.046123, -0.093405, 0.006513, -0.099163, -0.083334, 0.081412, 0.002355, -0.062226, 0.049188, -0.025471, -0.122051, -0.102297, -0.063403, -0.154102, 0.002556, -0.000293, 0.075304, -0.081595, 0.153943, 0.113328, -0.17462, 0.110954, -0.135872, -0.014036, 0.075627, -0.049148, 0.052463, 0.050913, -0.104945, 0.020758, 0.061778, -0.048521, -0.080983, 0.04208, -0.051486, -0.034994, 0.107299, 0.083745, 0.031438, 0.041436, 0.074622, -0.006988, -0.113035, -0.11442, 0.07532, -0.044607, 0.116584, -0.022137, 0.068219, 0.056347, 0.097191, -0.038671, -0.022925, 0.007269, 0.002257, -0.090832, 0.025369, -0.007991, 0.018976, -0.085273]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP015] Ngô Thanh Tùng',
        updated_at = GETDATE()
    WHERE employee_id = 15;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (15, '[0.09648, 0.028163, 0.071946, -0.026625, 0.059144, 0.128953, -0.089607, 0.00145, 0.034836, -0.110269, -0.079102, 0.120304, 0.19397, -0.165935, 0.019495, -0.032195, -0.100986, 0.030843, 0.172455, 0.040879, 0.074417, -0.081567, 0.012653, -0.01073, -0.130615, 0.002495, 0.064056, -0.079508, 0.083264, -0.115528, 0.06966, 0.085335, 0.049031, 0.115325, 0.035937, 0.061971, -0.020636, -0.029946, 0.038895, -0.074296, -0.340161, -0.095368, -0.146192, 0.00664, -0.045507, 0.006419, 0.053595, 0.059736, 0.013086, 0.055428, 0.042462, -0.034244, 0.067752, 0.017009, 0.14842, 0.139574, 0.075121, -0.215893, -0.006701, -0.04887, 0.096324, 0.031076, -0.123108, 0.212288, -0.108246, 0.014986, 0.038932, -0.081511, 0.046123, -0.093405, 0.006513, -0.099163, -0.083334, 0.081412, 0.002355, -0.062226, 0.049188, -0.025471, -0.122051, -0.102297, -0.063403, -0.154102, 0.002556, -0.000293, 0.075304, -0.081595, 0.153943, 0.113328, -0.17462, 0.110954, -0.135872, -0.014036, 0.075627, -0.049148, 0.052463, 0.050913, -0.104945, 0.020758, 0.061778, -0.048521, -0.080983, 0.04208, -0.051486, -0.034994, 0.107299, 0.083745, 0.031438, 0.041436, 0.074622, -0.006988, -0.113035, -0.11442, 0.07532, -0.044607, 0.116584, -0.022137, 0.068219, 0.056347, 0.097191, -0.038671, -0.022925, 0.007269, 0.002257, -0.090832, 0.025369, -0.007991, 0.018976, -0.085273]', DATEADD(DAY, -15, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP015] Ngô Thanh Tùng', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 16)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[-0.043073, 0.144647, 0.129391, 0.054558, 0.096798, 0.003839, -0.03507, 0.006856, 0.008429, -0.014135, -0.041135, 0.036525, -0.039073, 0.088918, 0.157096, -0.044165, -0.001243, -0.023196, -0.067386, 0.013177, 0.064181, -0.130815, -0.046416, -0.156253, 0.055988, 0.152167, -0.16803, -0.135984, -0.008336, -0.143577, -0.034509, -0.0156, 0.09256, -0.011706, 0.085225, -0.022173, 0.019425, -0.066388, 0.111485, -0.061384, 0.007337, -0.065309, -0.127727, 0.014884, 0.077594, 0.104269, 0.229278, -0.01233, -0.015105, 0.018701, 0.133694, -0.051862, -0.012854, -0.177372, 0.039599, -0.004746, -0.095468, -0.166445, -0.050296, 0.104675, 0.012672, 0.041022, -0.078742, 0.244149, -0.053452, 0.032484, 0.014469, 0.076945, 0.08393, 0.084768, -0.121236, -0.014752, 0.015317, -0.08448, -0.160965, 0.081378, 0.007243, 0.051339, -0.007979, -0.104583, 0.026093, -0.041416, 0.056301, -0.135396, -0.136349, -0.0195, 0.164501, 0.094044, 0.030668, 0.008784, -0.076833, 0.012681, 0.062941, 0.059134, 0.092192, -0.035829, 0.094479, 0.004871, 0.07303, 0.076829, -0.005812, -0.051604, -0.035043, -0.060317, -0.085325, 0.165222, 0.081446, 0.030469, 0.055375, -0.069072, -0.053419, 0.126925, -0.128061, -0.267, -0.143402, -0.069717, 0.176276, -0.077145, 0.045716, 0.05096, -0.055897, 0.002658, 0.007128, -0.024219, -0.038858, 0.011956, -0.003106, -0.079022]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP016] Dương Mỹ Hạnh',
        updated_at = GETDATE()
    WHERE employee_id = 16;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (16, '[-0.043073, 0.144647, 0.129391, 0.054558, 0.096798, 0.003839, -0.03507, 0.006856, 0.008429, -0.014135, -0.041135, 0.036525, -0.039073, 0.088918, 0.157096, -0.044165, -0.001243, -0.023196, -0.067386, 0.013177, 0.064181, -0.130815, -0.046416, -0.156253, 0.055988, 0.152167, -0.16803, -0.135984, -0.008336, -0.143577, -0.034509, -0.0156, 0.09256, -0.011706, 0.085225, -0.022173, 0.019425, -0.066388, 0.111485, -0.061384, 0.007337, -0.065309, -0.127727, 0.014884, 0.077594, 0.104269, 0.229278, -0.01233, -0.015105, 0.018701, 0.133694, -0.051862, -0.012854, -0.177372, 0.039599, -0.004746, -0.095468, -0.166445, -0.050296, 0.104675, 0.012672, 0.041022, -0.078742, 0.244149, -0.053452, 0.032484, 0.014469, 0.076945, 0.08393, 0.084768, -0.121236, -0.014752, 0.015317, -0.08448, -0.160965, 0.081378, 0.007243, 0.051339, -0.007979, -0.104583, 0.026093, -0.041416, 0.056301, -0.135396, -0.136349, -0.0195, 0.164501, 0.094044, 0.030668, 0.008784, -0.076833, 0.012681, 0.062941, 0.059134, 0.092192, -0.035829, 0.094479, 0.004871, 0.07303, 0.076829, -0.005812, -0.051604, -0.035043, -0.060317, -0.085325, 0.165222, 0.081446, 0.030469, 0.055375, -0.069072, -0.053419, 0.126925, -0.128061, -0.267, -0.143402, -0.069717, 0.176276, -0.077145, 0.045716, 0.05096, -0.055897, 0.002658, 0.007128, -0.024219, -0.038858, 0.011956, -0.003106, -0.079022]', DATEADD(DAY, -16, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP016] Dương Mỹ Hạnh', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 17)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[-0.003614, -0.026201, 0.024529, 0.036998, -0.060992, 0.057546, -0.069289, -0.225233, 0.176707, -0.067333, -0.117296, -0.077015, -0.033364, -0.058143, 0.060852, 0.105014, 0.100337, -0.069886, 0.069269, 0.051491, -0.142556, -0.095662, -0.12874, 0.111616, 0.042673, 0.047809, 0.121312, 0.033734, -0.030698, 0.02119, 0.113257, -0.053169, 0.100029, 0.00323, 0.04712, 0.024276, -0.075153, 0.158787, -0.004487, 0.058916, 0.115, -0.119004, 0.097135, -0.203477, -0.040502, -0.062595, -0.022507, 0.064037, -0.124823, -0.032092, -0.079452, 0.132472, -0.025429, 0.079699, 0.01467, -0.057971, -0.050857, -0.066643, 0.055372, -0.028141, 0.023832, -0.042827, -0.06395, -0.024996, 0.064917, -0.043976, -0.021971, 0.024602, -0.036308, 0.012193, 0.122901, -0.050667, 0.081807, 0.132003, -0.167069, -0.128283, -0.008245, 0.033085, 0.11194, 0.075806, 0.068098, -0.0319, -0.09359, -0.063465, -0.171275, 0.037758, -0.036182, -0.032687, 0.067989, -0.131443, -0.14334, -0.038743, -0.096868, 0.080853, 0.071728, -0.226013, 0.007141, 0.067694, 0.028012, 0.027611, 0.143557, 0.107411, -0.176692, -0.037828, 0.026125, 0.030513, -0.095526, -0.015174, -0.030654, 0.085805, 0.073274, 0.157949, -0.211533, 0.147864, 0.0124, 0.100139, 0.100562, 0.005433, 0.06917, 0.086715, 0.041744, -0.065166, 0.021907, 0.023981, 0.030302, 0.109909, -0.123459, 0.022351]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP017] Trịnh Quốc Bảo',
        updated_at = GETDATE()
    WHERE employee_id = 17;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (17, '[-0.003614, -0.026201, 0.024529, 0.036998, -0.060992, 0.057546, -0.069289, -0.225233, 0.176707, -0.067333, -0.117296, -0.077015, -0.033364, -0.058143, 0.060852, 0.105014, 0.100337, -0.069886, 0.069269, 0.051491, -0.142556, -0.095662, -0.12874, 0.111616, 0.042673, 0.047809, 0.121312, 0.033734, -0.030698, 0.02119, 0.113257, -0.053169, 0.100029, 0.00323, 0.04712, 0.024276, -0.075153, 0.158787, -0.004487, 0.058916, 0.115, -0.119004, 0.097135, -0.203477, -0.040502, -0.062595, -0.022507, 0.064037, -0.124823, -0.032092, -0.079452, 0.132472, -0.025429, 0.079699, 0.01467, -0.057971, -0.050857, -0.066643, 0.055372, -0.028141, 0.023832, -0.042827, -0.06395, -0.024996, 0.064917, -0.043976, -0.021971, 0.024602, -0.036308, 0.012193, 0.122901, -0.050667, 0.081807, 0.132003, -0.167069, -0.128283, -0.008245, 0.033085, 0.11194, 0.075806, 0.068098, -0.0319, -0.09359, -0.063465, -0.171275, 0.037758, -0.036182, -0.032687, 0.067989, -0.131443, -0.14334, -0.038743, -0.096868, 0.080853, 0.071728, -0.226013, 0.007141, 0.067694, 0.028012, 0.027611, 0.143557, 0.107411, -0.176692, -0.037828, 0.026125, 0.030513, -0.095526, -0.015174, -0.030654, 0.085805, 0.073274, 0.157949, -0.211533, 0.147864, 0.0124, 0.100139, 0.100562, 0.005433, 0.06917, 0.086715, 0.041744, -0.065166, 0.021907, 0.023981, 0.030302, 0.109909, -0.123459, 0.022351]', DATEADD(DAY, -17, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP017] Trịnh Quốc Bảo', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 18)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[0.018402, 0.007814, 0.026767, -0.005528, 0.038137, -0.014196, -0.015505, -0.032794, -0.009474, -0.024594, -0.084848, 0.008847, -0.074629, -0.057519, 0.011607, 0.035566, -0.051177, -0.036352, 0.134971, -0.05082, -0.093705, 0.035649, 0.190643, 0.070462, -0.018464, 0.037867, -0.056284, -0.066296, -0.054984, -0.000302, 0.035505, 0.236389, 0.016128, -0.124163, 0.029592, 0.104057, -0.110153, -0.133367, -0.039996, -0.12949, 0.267818, 0.003312, -0.040842, -0.001031, -0.074711, 0.033839, -0.002683, 0.110548, -0.006421, 0.07048, -0.056679, 0.058587, 0.070742, 0.005545, -0.092726, 0.072502, -0.166704, -0.099425, 0.287019, 0.114371, 0.103566, -0.008957, 0.12566, 0.121415, 0.043689, -0.007243, -0.002164, -0.003347, -0.123421, 0.001835, 0.106929, 0.104869, 0.114635, 0.161177, -0.144192, -0.026114, -0.036866, -0.039354, 0.19995, 0.007074, 0.067101, -0.01061, 0.020277, 0.007216, -0.001097, -0.029184, -0.051748, -0.049487, -0.029923, -0.001243, -0.121154, 0.048682, -0.09582, -0.096609, 0.068529, -0.039187, -0.061019, -0.014081, -0.001147, 0.026686, 0.090813, 0.039291, 0.091443, -0.107706, 0.069987, 0.044267, -0.070546, -0.117646, 0.059387, 0.00029, -0.139954, -0.048999, -0.074882, -0.063083, -0.034787, -0.075027, 0.058396, -0.0676, -0.129955, -0.015904, -0.202729, 0.126444, 0.029503, -0.049251, -0.022949, -0.156427, 0.206963, -0.081961]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP018] Cao Thu Trang',
        updated_at = GETDATE()
    WHERE employee_id = 18;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (18, '[0.018402, 0.007814, 0.026767, -0.005528, 0.038137, -0.014196, -0.015505, -0.032794, -0.009474, -0.024594, -0.084848, 0.008847, -0.074629, -0.057519, 0.011607, 0.035566, -0.051177, -0.036352, 0.134971, -0.05082, -0.093705, 0.035649, 0.190643, 0.070462, -0.018464, 0.037867, -0.056284, -0.066296, -0.054984, -0.000302, 0.035505, 0.236389, 0.016128, -0.124163, 0.029592, 0.104057, -0.110153, -0.133367, -0.039996, -0.12949, 0.267818, 0.003312, -0.040842, -0.001031, -0.074711, 0.033839, -0.002683, 0.110548, -0.006421, 0.07048, -0.056679, 0.058587, 0.070742, 0.005545, -0.092726, 0.072502, -0.166704, -0.099425, 0.287019, 0.114371, 0.103566, -0.008957, 0.12566, 0.121415, 0.043689, -0.007243, -0.002164, -0.003347, -0.123421, 0.001835, 0.106929, 0.104869, 0.114635, 0.161177, -0.144192, -0.026114, -0.036866, -0.039354, 0.19995, 0.007074, 0.067101, -0.01061, 0.020277, 0.007216, -0.001097, -0.029184, -0.051748, -0.049487, -0.029923, -0.001243, -0.121154, 0.048682, -0.09582, -0.096609, 0.068529, -0.039187, -0.061019, -0.014081, -0.001147, 0.026686, 0.090813, 0.039291, 0.091443, -0.107706, 0.069987, 0.044267, -0.070546, -0.117646, 0.059387, 0.00029, -0.139954, -0.048999, -0.074882, -0.063083, -0.034787, -0.075027, 0.058396, -0.0676, -0.129955, -0.015904, -0.202729, 0.126444, 0.029503, -0.049251, -0.022949, -0.156427, 0.206963, -0.081961]', DATEADD(DAY, -18, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP018] Cao Thu Trang', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 19)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[0.12131, -0.133376, -0.134947, 0.030193, -0.239009, 0.056937, 0.107846, 0.096901, -0.036593, 0.018615, 0.020338, 0.021732, -0.102327, 0.05665, 0.047609, -0.026729, -0.035824, 0.158786, -0.07321, 0.000927, -0.07432, 0.079339, -0.040685, 0.029223, 0.162555, 0.010175, -0.009004, 0.129436, -0.01174, -0.174717, -0.026266, 0.148693, -0.05847, -0.002986, -0.05517, 0.003744, -0.09429, -0.038336, 0.0035, 0.057272, -0.048048, 0.040317, 0.162491, -0.109478, 0.082651, -0.104869, -0.114969, 0.042609, -0.053385, 0.137087, 0.214492, -0.058605, 0.003361, 0.099147, -0.03352, -0.11579, 0.007499, 0.033188, -0.022126, -0.05909, 0.069807, -0.14008, -0.069656, 0.003623, 0.0375, -0.076405, -0.020322, -0.05232, 0.071685, 0.067841, -0.014094, 0.110101, -0.01865, 0.095381, 0.030653, 0.022519, -0.130068, -0.005043, 0.141344, 0.052936, 0.152132, 0.081793, 0.108602, 0.024898, 0.221373, -0.084931, -0.028643, 0.035637, -0.110936, -0.037982, -0.047509, 0.08399, 0.070154, -0.02131, -0.049218, -0.147067, 0.063023, 0.111455, 0.063211, -0.098538, -0.148971, -0.053039, -0.064819, -0.036758, 0.0199, -0.056584, 0.004636, -0.026868, 0.020807, 0.031792, 0.03032, -0.0061, 0.052181, 0.019853, -0.081895, -0.051193, -0.15007, 0.166414, -0.077812, 0.047502, -0.000625, 0.036185, 0.03149, 0.018567, -0.126937, -0.206567, -0.172489, 0.117453]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP019] Mai Hoàng Long',
        updated_at = GETDATE()
    WHERE employee_id = 19;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (19, '[0.12131, -0.133376, -0.134947, 0.030193, -0.239009, 0.056937, 0.107846, 0.096901, -0.036593, 0.018615, 0.020338, 0.021732, -0.102327, 0.05665, 0.047609, -0.026729, -0.035824, 0.158786, -0.07321, 0.000927, -0.07432, 0.079339, -0.040685, 0.029223, 0.162555, 0.010175, -0.009004, 0.129436, -0.01174, -0.174717, -0.026266, 0.148693, -0.05847, -0.002986, -0.05517, 0.003744, -0.09429, -0.038336, 0.0035, 0.057272, -0.048048, 0.040317, 0.162491, -0.109478, 0.082651, -0.104869, -0.114969, 0.042609, -0.053385, 0.137087, 0.214492, -0.058605, 0.003361, 0.099147, -0.03352, -0.11579, 0.007499, 0.033188, -0.022126, -0.05909, 0.069807, -0.14008, -0.069656, 0.003623, 0.0375, -0.076405, -0.020322, -0.05232, 0.071685, 0.067841, -0.014094, 0.110101, -0.01865, 0.095381, 0.030653, 0.022519, -0.130068, -0.005043, 0.141344, 0.052936, 0.152132, 0.081793, 0.108602, 0.024898, 0.221373, -0.084931, -0.028643, 0.035637, -0.110936, -0.037982, -0.047509, 0.08399, 0.070154, -0.02131, -0.049218, -0.147067, 0.063023, 0.111455, 0.063211, -0.098538, -0.148971, -0.053039, -0.064819, -0.036758, 0.0199, -0.056584, 0.004636, -0.026868, 0.020807, 0.031792, 0.03032, -0.0061, 0.052181, 0.019853, -0.081895, -0.051193, -0.15007, 0.166414, -0.077812, 0.047502, -0.000625, 0.036185, 0.03149, 0.018567, -0.126937, -0.206567, -0.172489, 0.117453]', DATEADD(DAY, -19, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP019] Mai Hoàng Long', GETDATE());
END


IF EXISTS (SELECT 1 FROM employee_face_profiles WHERE employee_id = 20)
BEGIN
    UPDATE employee_face_profiles
    SET face_descriptor_json = '[-0.125088, 0.077876, 0.002966, 0.000249, -0.049432, -0.022092, -0.02243, 0.096783, 0.07518, 0.020818, -0.008204, -0.014612, -0.01092, -0.055998, 0.108091, -0.037875, -0.181968, -0.088178, -0.022878, 0.103304, 0.072785, -0.016982, 0.02614, -0.003477, -0.237059, 0.008859, 0.044087, -0.075349, 0.008968, 0.040857, 0.014909, 0.161454, -0.074921, 0.027385, 0.043214, -0.135844, -0.027439, -0.032903, -0.078965, 0.008349, -0.098284, 0.178629, 0.110298, 0.223296, -0.062582, 0.04932, -0.19156, 0.13041, 0.134106, 0.010762, -0.08899, 0.02977, -0.020972, 0.146773, -0.132974, 0.002203, 0.055889, 0.065333, 0.01835, 0.069723, -0.042394, -0.066095, 0.057362, -0.02935, -0.065714, -0.041745, 0.008372, 0.070269, -0.005524, -0.109309, 0.042061, 0.08597, 0.016145, 0.046475, -0.186826, -0.13532, 0.062866, 0.007455, -0.074159, -0.031708, -0.062683, 0.006376, -0.01016, -0.143613, 0.042687, -0.066368, -0.019785, 0.171367, -0.080374, 0.07283, 0.010847, -0.257872, -0.022194, 0.075285, -0.118737, 0.071036, 0.043539, -0.027245, 0.031648, 0.042712, 0.066459, -0.113047, 0.073424, 0.072133, 0.17206, 0.062625, 0.04269, -0.078985, 0.021554, -0.041699, -0.07346, -0.114035, 0.027643, -0.14353, 0.033144, 0.115399, 0.12204, 0.018012, -0.114024, -0.011943, 0.099248, 0.073854, -0.105696, -0.095422, 0.001877, -0.012811, 0.088975, 0.196231]',
        is_active = 1,
        liveness_threshold = 0.70,
        notes = N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP020] Võ Như Quỳnh',
        updated_at = GETDATE()
    WHERE employee_id = 20;
END
ELSE
BEGIN
    INSERT INTO employee_face_profiles (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
    VALUES (20, '[-0.125088, 0.077876, 0.002966, 0.000249, -0.049432, -0.022092, -0.02243, 0.096783, 0.07518, 0.020818, -0.008204, -0.014612, -0.01092, -0.055998, 0.108091, -0.037875, -0.181968, -0.088178, -0.022878, 0.103304, 0.072785, -0.016982, 0.02614, -0.003477, -0.237059, 0.008859, 0.044087, -0.075349, 0.008968, 0.040857, 0.014909, 0.161454, -0.074921, 0.027385, 0.043214, -0.135844, -0.027439, -0.032903, -0.078965, 0.008349, -0.098284, 0.178629, 0.110298, 0.223296, -0.062582, 0.04932, -0.19156, 0.13041, 0.134106, 0.010762, -0.08899, 0.02977, -0.020972, 0.146773, -0.132974, 0.002203, 0.055889, 0.065333, 0.01835, 0.069723, -0.042394, -0.066095, 0.057362, -0.02935, -0.065714, -0.041745, 0.008372, 0.070269, -0.005524, -0.109309, 0.042061, 0.08597, 0.016145, 0.046475, -0.186826, -0.13532, 0.062866, 0.007455, -0.074159, -0.031708, -0.062683, 0.006376, -0.01016, -0.143613, 0.042687, -0.066368, -0.019785, 0.171367, -0.080374, 0.07283, 0.010847, -0.257872, -0.022194, 0.075285, -0.118737, 0.071036, 0.043539, -0.027245, 0.031648, 0.042712, 0.066459, -0.113047, 0.073424, 0.072133, 0.17206, 0.062625, 0.04269, -0.078985, 0.021554, -0.041699, -0.07346, -0.114035, 0.027643, -0.14353, 0.033144, 0.115399, 0.12204, 0.018012, -0.114024, -0.011943, 0.099248, 0.073854, -0.105696, -0.095422, 0.001877, -0.012811, 0.088975, 0.196231]', DATEADD(DAY, -20, GETDATE()), 'admin.trong', 1, 0.70, N'Hồ sơ sinh trắc học chuẩn hóa 128-D nhân viên [EMP020] Võ Như Quỳnh', GETDATE());
END
GO

-- 8. KIỂM TRA TỔNG KẾT DỮ LIỆU HỆ THỐNG
SELECT 
    (SELECT COUNT(*) FROM departments) AS TongPhongBan,
    (SELECT COUNT(*) FROM employees) AS TongNhanSu,
    (SELECT COUNT(*) FROM user_accounts) AS TongTaiKhoan,
    (SELECT COUNT(*) FROM contracts) AS TongHopDong,
    (SELECT COUNT(*) FROM work_shifts) AS TongCaLamViec,
    (SELECT COUNT(*) FROM shift_assignments) AS TongLichPhanCa,
    (SELECT COUNT(*) FROM timesheets) AS TongBanGhiChamCong,
    (SELECT COUNT(*) FROM attendance_requests) AS TongYeuCauChamCong,
    (SELECT COUNT(*) FROM leave_requests) AS TongDonNghiPhep,
    (SELECT COUNT(*) FROM payslips) AS TongPhieuLuong;
GO
