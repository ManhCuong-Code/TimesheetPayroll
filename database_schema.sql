-- ============================================================================
-- DỰ ÁN: HỆ THỐNG CHẤM CÔNG & TÍNH LƯƠNG TỰ ĐỘNG (TIMESHEET & PAYROLL SYSTEM)
-- CƠ SỞ DỮ LIỆU RIÊNG BIỆT: BangChamCongDB
-- (ĐÃ TÁCH BIỆT HOÀN TOÀN, ĐỘC LẬP 100% VỚI DỰ ÁN HOMESTAY HUẾ - HomeStayHueDB)
-- Môi trường: Microsoft SQL Server / SQL Server Management Studio (SSMS)
-- ============================================================================

-- BƯỚC 1: TẠO VÀ SỬ DỤNG CSDL RIÊNG 'BangChamCongDB'
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BangChamCongDB')
BEGIN
    CREATE DATABASE BangChamCongDB;
END
GO

USE BangChamCongDB;
GO

-- ============================================================================
-- BƯỚC 2: XÓA BẢNG CŨ (NẾU CÓ) THEO THỨ TỰ PHỤ THUỘC TRONG BangChamCongDB
-- Tuyệt đối không ảnh hưởng đến bất kỳ CSDL nào khác (như HomeStayHueDB)
-- ============================================================================
IF OBJECT_ID('dbo.payslip_lines', 'U') IS NOT NULL DROP TABLE dbo.payslip_lines;
IF OBJECT_ID('dbo.payslips', 'U') IS NOT NULL DROP TABLE dbo.payslips;
IF OBJECT_ID('dbo.leave_requests', 'U') IS NOT NULL DROP TABLE dbo.leave_requests;
IF OBJECT_ID('dbo.timesheets', 'U') IS NOT NULL DROP TABLE dbo.timesheets;
IF OBJECT_ID('dbo.payroll_periods', 'U') IS NOT NULL DROP TABLE dbo.payroll_periods;
IF OBJECT_ID('dbo.deduction_rates', 'U') IS NOT NULL DROP TABLE dbo.deduction_rates;
IF OBJECT_ID('dbo.contracts', 'U') IS NOT NULL DROP TABLE dbo.contracts;
IF OBJECT_ID('dbo.user_accounts', 'U') IS NOT NULL DROP TABLE dbo.user_accounts;
IF OBJECT_ID('dbo.employees', 'U') IS NOT NULL DROP TABLE dbo.employees;
IF OBJECT_ID('dbo.departments', 'U') IS NOT NULL DROP TABLE dbo.departments;
GO

-- ============================================================================
-- BƯỚC 3: TẠO 10 BẢNG NGHIỆP VỤ CHẤM CÔNG & TÍNH LƯƠNG
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 1. BẢNG PHÒNG BAN (departments)
-- ----------------------------------------------------------------------------
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

-- ----------------------------------------------------------------------------
-- 2. BẢNG HỒ SƠ NHÂN VIÊN (employees)
-- ----------------------------------------------------------------------------
CREATE TABLE employees (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    employee_code NVARCHAR(30) NOT NULL UNIQUE,
    full_name NVARCHAR(100) NOT NULL,
    date_of_birth DATE NULL,
    gender NVARCHAR(10) NULL,
    identity_card_number NVARCHAR(20) NOT NULL UNIQUE, -- CCCD / CMND
    email NVARCHAR(120) NOT NULL UNIQUE,
    phone_number NVARCHAR(20) NULL,
    bank_account_number NVARCHAR(50) NOT NULL,         -- Số tài khoản nhận lương
    bank_name NVARCHAR(100) NOT NULL,                  -- Ngân hàng nhận lương
    department_id BIGINT NULL CONSTRAINT fk_employees_dept REFERENCES departments(id) ON DELETE SET NULL,
    hire_date DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    status NVARCHAR(30) NOT NULL DEFAULT 'ACTIVE' CHECK (status IN ('ACTIVE', 'ON_LEAVE', 'RESIGNED')),
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- ----------------------------------------------------------------------------
-- 3. BẢNG TÀI KHOẢN ĐĂNG NHẬP (user_accounts) - Quan hệ 1-1 với employees
-- ----------------------------------------------------------------------------
CREATE TABLE user_accounts (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    employee_id BIGINT NOT NULL UNIQUE CONSTRAINT fk_users_emp REFERENCES employees(id) ON DELETE CASCADE,
    username NVARCHAR(50) NOT NULL UNIQUE,
    password_hash NVARCHAR(255) NOT NULL,
    role NVARCHAR(50) NOT NULL DEFAULT 'ROLE_EMPLOYEE' CHECK (role IN ('ROLE_EMPLOYEE', 'ROLE_HR_ACCOUNTANT', 'ROLE_ADMIN')),
    is_active BIT NOT NULL DEFAULT 1,
    last_login_at DATETIME2 NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- ----------------------------------------------------------------------------
-- 4. BẢNG HỢP ĐỒNG LAO ĐỘNG (contracts)
-- ----------------------------------------------------------------------------
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

-- ----------------------------------------------------------------------------
-- 5. BẢNG KỲ TÍNH LƯƠNG (payroll_periods)
-- ----------------------------------------------------------------------------
CREATE TABLE payroll_periods (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    period_code NVARCHAR(30) NOT NULL UNIQUE,         -- Ví dụ: PR-2026-10
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

-- ----------------------------------------------------------------------------
-- 6. BẢNG CẤU HÌNH TỶ LỆ TRÍCH NỘP BẢO HIỂM (deduction_rates)
-- ----------------------------------------------------------------------------
CREATE TABLE deduction_rates (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    rate_code NVARCHAR(30) NOT NULL UNIQUE,            -- BHXH, BHYT, BHTN
    rate_name NVARCHAR(100) NOT NULL,
    employee_rate DECIMAL(6, 4) NOT NULL CHECK (employee_rate >= 0), -- 0.0800 (8%)
    employer_rate DECIMAL(6, 4) NOT NULL CHECK (employer_rate >= 0), -- 0.1750 (17.5%)
    effective_from DATE NOT NULL,
    effective_to DATE NULL,
    is_active BIT NOT NULL DEFAULT 1,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT chk_deduction_dates CHECK (effective_to IS NULL OR effective_to >= effective_from)
);
GO

-- ----------------------------------------------------------------------------
-- 7. BẢNG CHẤM CÔNG HÀNG NGÀY (timesheets)
-- ----------------------------------------------------------------------------
CREATE TABLE timesheets (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    employee_id BIGINT NOT NULL CONSTRAINT fk_timesheets_emp REFERENCES employees(id) ON DELETE CASCADE,
    period_id BIGINT NULL CONSTRAINT fk_timesheets_period REFERENCES payroll_periods(id) ON DELETE NO ACTION,
    work_date DATE NOT NULL,
    check_in_time TIME NULL,
    check_out_time TIME NULL,
    actual_hours_worked DECIMAL(4, 2) NOT NULL DEFAULT 0.00 CHECK (actual_hours_worked >= 0),
    work_shift_status NVARCHAR(50) NOT NULL DEFAULT 'PRESENT' CHECK (work_shift_status IN ('PRESENT', 'LATE', 'EARLY_LEAVE', 'HALF_DAY', 'ABSENT_WITH_PERMISSION', 'ABSENT_UNAUTHORIZED', 'HOLIDAY')),
    work_units DECIMAL(3, 2) NOT NULL DEFAULT 1.00 CHECK (work_units >= 0 AND work_units <= 2.0),
    is_locked BIT NOT NULL DEFAULT 0,
    note NVARCHAR(255) NULL,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT uq_emp_work_date UNIQUE (employee_id, work_date)
);
GO

-- ----------------------------------------------------------------------------
-- 8. BẢNG ĐƠN XIN NGHỈ PHÉP (leave_requests)
-- ----------------------------------------------------------------------------
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

-- ----------------------------------------------------------------------------
-- 9. BẢNG PHIẾU LƯƠNG NHÂN VIÊN (payslips)
-- ----------------------------------------------------------------------------
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

-- ----------------------------------------------------------------------------
-- 10. BẢNG DÒNG CHI TIẾT PHIẾU LƯƠNG (payslip_lines)
-- ----------------------------------------------------------------------------
CREATE TABLE payslip_lines (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    payslip_id BIGINT NOT NULL CONSTRAINT fk_lines_payslip REFERENCES payslips(id) ON DELETE CASCADE,
    line_type NVARCHAR(50) NOT NULL CHECK (line_type IN ('BASE_SALARY_PRORATED', 'MEAL_ALLOWANCE', 'FUEL_ALLOWANCE', 'OVERTIME_PAY', 'BONUS', 'DEDUCTION_BHXH', 'DEDUCTION_BHYT', 'DEDUCTION_BHTN', 'PERSONAL_INCOME_TAX', 'PENALTY')),
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
CREATE INDEX idx_payslips_period_emp ON payslips(period_id, employee_id);
CREATE INDEX idx_payslips_employee ON payslips(employee_id);
CREATE INDEX idx_payslip_lines_payslip ON payslip_lines(payslip_id);
GO

-- ============================================================================
-- BƯỚC 5: NẠP DỮ LIỆU MẪU CHUẨN VÀO BangChamCongDB
-- ============================================================================

-- 5.1. Thêm phòng ban
SET IDENTITY_INSERT departments ON;
INSERT INTO departments (id, department_code, department_name, manager_name, description) VALUES
(1, N'DEP-IT', N'Phòng Công Nghệ Thông Tin', N'Trần Đình Trọng', N'Phát triển phần mềm và hạ tầng hệ thống'),
(2, N'DEP-HR', N'Phòng Hành Chính Nhân Sự & Kế Toán', N'Lê Thị Thu Thảo', N'Quản lý nhân sự, chấm công và chi trả tiền lương'),
(3, N'DEP-SALES', N'Phòng Kinh Doanh & Tiếp Thị', N'Phạm Hoàng Nam', N'Phát triển khách hàng và doanh thu công ty');
SET IDENTITY_INSERT departments OFF;
GO

-- 5.2. Thêm tỷ lệ khấu trừ bảo hiểm bắt buộc (Luật Việt Nam: 10.5%)
SET IDENTITY_INSERT deduction_rates ON;
INSERT INTO deduction_rates (id, rate_code, rate_name, employee_rate, employer_rate, effective_from, effective_to, is_active) VALUES
(1, N'BHXH', N'Bảo hiểm Xã hội', 0.0800, 0.1750, '2026-01-01', NULL, 1),
(2, N'BHYT', N'Bảo hiểm Y tế', 0.0150, 0.0300, '2026-01-01', NULL, 1),
(3, N'BHTN', N'Bảo hiểm Thất nghiệp', 0.0100, 0.0100, '2026-01-01', NULL, 1);
SET IDENTITY_INSERT deduction_rates OFF;
GO

-- 5.3. Thêm hồ sơ nhân viên
SET IDENTITY_INSERT employees ON;
INSERT INTO employees (id, employee_code, full_name, date_of_birth, gender, identity_card_number, email, phone_number, bank_account_number, bank_name, department_id, hire_date, status) VALUES
(1, N'EMP001', N'Nguyễn Văn A', '1995-05-15', N'Nam', N'001095012345', N'nguyenvana@congty.com', N'0912345678', N'19035678901234', N'Vietcombank', 1, '2023-01-10', 'ACTIVE'),
(2, N'EMP002', N'Lê Thị Thu Thảo', '1992-08-20', N'Nữ', N'001092054321', N'thaolt@congty.com', N'0987654321', N'00210003456789', N'MB Bank', 2, '2022-03-01', 'ACTIVE'),
(3, N'EMP003', N'Trần Đình Trọng', '1988-11-12', N'Nam', N'001088099887', N'trongtd@congty.com', N'0903112233', N'10123498765432', N'Techcombank', 1, '2021-06-15', 'ACTIVE'),
(4, N'EMP004', N'Hoàng Minh Tuấn', '1998-02-28', N'Nam', N'001098044556', N'tuanhm@congty.com', N'0977889900', N'12400008899776', N'BIDV', 3, '2024-02-01', 'ACTIVE');
SET IDENTITY_INSERT employees OFF;
GO

-- 5.4. Thêm tài khoản người dùng tương ứng
SET IDENTITY_INSERT user_accounts ON;
INSERT INTO user_accounts (id, employee_id, username, password_hash, role, is_active) VALUES
(1, 1, N'nhanvien.a', N'$2a$10$EblZqNptyYvcLm/VwDCVAuBjzZOI7khzdyGPBr08PpIi0na624b8.', 'ROLE_EMPLOYEE', 1),
(2, 2, N'ketoan.thao', N'$2a$10$EblZqNptyYvcLm/VwDCVAuBjzZOI7khzdyGPBr08PpIi0na624b8.', 'ROLE_HR_ACCOUNTANT', 1),
(3, 3, N'admin.trong', N'$2a$10$EblZqNptyYvcLm/VwDCVAuBjzZOI7khzdyGPBr08PpIi0na624b8.', 'ROLE_ADMIN', 1),
(4, 4, N'nhanvien.tuan', N'$2a$10$EblZqNptyYvcLm/VwDCVAuBjzZOI7khzdyGPBr08PpIi0na624b8.', 'ROLE_EMPLOYEE', 1);
SET IDENTITY_INSERT user_accounts OFF;
GO

-- 5.5. Thêm hợp đồng lao động
SET IDENTITY_INSERT contracts ON;
INSERT INTO contracts (id, employee_id, contract_number, contract_type, base_salary, meal_allowance, fuel_allowance, start_date, end_date, is_active) VALUES
(1, 1, N'HDLD-2023-001', 'INDEFINITE', 10000000.00, 500000.00, 0.00, '2023-01-10', NULL, 1),
(2, 2, N'HDLD-2022-015', 'INDEFINITE', 15000000.00, 500000.00, 500000.00, '2022-03-01', NULL, 1),
(3, 3, N'HDLD-2021-008', 'INDEFINITE', 25000000.00, 500000.00, 1000000.00, '2021-06-15', NULL, 1),
(4, 4, N'HDLD-2024-032', 'FULL_TIME', 8000000.00, 500000.00, 0.00, '2024-02-01', '2025-02-01', 1);
SET IDENTITY_INSERT contracts OFF;
GO

-- 5.6. Thêm kỳ lương Tháng 10/2026
SET IDENTITY_INSERT payroll_periods ON;
INSERT INTO payroll_periods (id, period_code, period_month, period_year, standard_work_days, status, total_gross_amount, total_net_amount) VALUES
(10, N'PR-2026-10', 10, 2026, 22.00, 'CALCULATED', 59590909.00, 53289090.00);
SET IDENTITY_INSERT payroll_periods OFF;
GO

-- 5.7. Thêm bảng công tháng 10/2026 cho EMP001 (Nguyễn Văn A đi làm 20 ngày)
;WITH DaysCTE AS (
    SELECT 0 AS d
    UNION ALL
    SELECT d + 1 FROM DaysCTE WHERE d < 19
)
INSERT INTO timesheets (employee_id, period_id, work_date, check_in_time, check_out_time, actual_hours_worked, work_shift_status, work_units, is_locked)
SELECT 
    1, 10, DATEADD(DAY, d, '2026-10-01'),
    '08:00:00', '17:30:00', 8.0, 'PRESENT', 1.0, 0
FROM DaysCTE;

-- Thêm 2 ngày nghỉ việc riêng có phép cho EMP001
INSERT INTO timesheets (employee_id, period_id, work_date, check_in_time, check_out_time, actual_hours_worked, work_shift_status, work_units, is_locked, note) VALUES
(1, 10, '2026-10-21', NULL, NULL, 0.0, 'ABSENT_WITH_PERMISSION', 0.0, 0, N'Nghỉ việc riêng có phép'),
(1, 10, '2026-10-22', NULL, NULL, 0.0, 'ABSENT_WITH_PERMISSION', 0.0, 0, N'Nghỉ việc riêng có phép');
GO

-- 5.8. Thêm đơn xin nghỉ phép của EMP001
SET IDENTITY_INSERT leave_requests ON;
INSERT INTO leave_requests (id, employee_id, leave_type, start_date, end_date, total_days, reason, approval_status, approved_by, approved_at) VALUES
(1, 1, 'UNPAID_LEAVE', '2026-10-21', '2026-10-22', 2.0, N'Giải quyết việc gia đình', 'APPROVED', N'Lê Thị Thu Thảo', '2026-10-20 15:30:00');
SET IDENTITY_INSERT leave_requests OFF;
GO

-- 5.9. Thêm Phiếu lương tháng 10/2026 cho Nguyễn Văn A
-- Lương công: 10.000.000 * 20 / 22 = 9.090.909 đ
-- Phụ cấp ăn: 500.000 đ => Gross = 9.590.909 đ
-- Trừ bảo hiểm (10.5%): 1.050.000 đ
-- Thực lĩnh (Net): 8.540.909 đ
SET IDENTITY_INSERT payslips ON;
INSERT INTO payslips (id, period_id, employee_id, actual_work_days, paid_leave_days, overtime_hours, gross_salary, total_deductions, net_salary) VALUES
(101, 10, 1, 20.0, 0.0, 0.0, 9590909.00, 1050000.00, 8540909.00);
SET IDENTITY_INSERT payslips OFF;
GO

-- 5.10. Thêm các dòng chi tiết cấu thành phiếu lương của Nguyễn Văn A
SET IDENTITY_INSERT payslip_lines ON;
INSERT INTO payslip_lines (id, payslip_id, line_type, line_description, amount, is_deduction) VALUES
(1, 101, 'BASE_SALARY_PRORATED', N'Lương ngày công thực tế (20/22 ngày)', 9090909.00, 0),
(2, 101, 'MEAL_ALLOWANCE', N'Phụ cấp tiền ăn trưa', 500000.00, 0),
(3, 101, 'DEDUCTION_BHXH', N'Trích nộp BHXH (8.0%)', 800000.00, 1),
(4, 101, 'DEDUCTION_BHYT', N'Trích nộp BHYT (1.5%)', 150000.00, 1),
(5, 101, 'DEDUCTION_BHTN', N'Trích nộp BHTN (1.0%)', 100000.00, 1);
SET IDENTITY_INSERT payslip_lines OFF;
GO

-- ============================================================================
-- BƯỚC 6: KIỂM TRA TRUY VẤN DỮ LIỆU ĐỐI SOÁT TRONG BangChamCongDB
-- ============================================================================
SELECT 
    p.id AS PayslipId,
    e.employee_code AS MaNV,
    e.full_name AS HoTen,
    d.department_name AS PhongBan,
    p.actual_work_days AS NgayCong,
    p.gross_salary AS LuongGop,
    p.total_deductions AS TongKhauTru,
    p.net_salary AS ThucLinh,
    e.bank_account_number AS SoTK,
    e.bank_name AS NganHang
FROM payslips p
INNER JOIN employees e ON p.employee_id = e.id
LEFT JOIN departments d ON e.department_id = d.id
WHERE p.period_id = 10;
GO
