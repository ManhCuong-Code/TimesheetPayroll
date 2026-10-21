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
