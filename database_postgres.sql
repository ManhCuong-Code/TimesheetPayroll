-- ============================================================================
-- DỰ ÁN: HỆ THỐNG CHẤM CÔNG & TÍNH LƯƠNG TỰ ĐỘNG (TIMESHEET & PAYROLL SYSTEM)
-- BẢN DÀNH CHO: POSTGRESQL 16 (DBeaver / pgAdmin 4 / psql)
-- ============================================================================

DROP TABLE IF EXISTS payslip_lines CASCADE;
DROP TABLE IF EXISTS payslips CASCADE;
DROP TABLE IF EXISTS leave_requests CASCADE;
DROP TABLE IF EXISTS timesheets CASCADE;
DROP TABLE IF EXISTS payroll_periods CASCADE;
DROP TABLE IF EXISTS deduction_rates CASCADE;
DROP TABLE IF EXISTS contracts CASCADE;
DROP TABLE IF EXISTS user_accounts CASCADE;
DROP TABLE IF EXISTS employees CASCADE;
DROP TABLE IF EXISTS departments CASCADE;

DROP TYPE IF EXISTS user_role_enum CASCADE;
DROP TYPE IF EXISTS work_shift_status_enum CASCADE;
DROP TYPE IF EXISTS leave_type_enum CASCADE;
DROP TYPE IF EXISTS approval_status_enum CASCADE;
DROP TYPE IF EXISTS period_status_enum CASCADE;
DROP TYPE IF EXISTS payslip_line_type_enum CASCADE;

CREATE TYPE user_role_enum AS ENUM ('ROLE_EMPLOYEE', 'ROLE_HR_ACCOUNTANT', 'ROLE_ADMIN');
CREATE TYPE work_shift_status_enum AS ENUM ('PRESENT', 'LATE', 'EARLY_LEAVE', 'HALF_DAY', 'ABSENT_WITH_PERMISSION', 'ABSENT_UNAUTHORIZED', 'HOLIDAY');
CREATE TYPE leave_type_enum AS ENUM ('ANNUAL_LEAVE', 'SICK_LEAVE', 'MATERNITY_LEAVE', 'UNPAID_LEAVE');
CREATE TYPE approval_status_enum AS ENUM ('PENDING', 'APPROVED', 'REJECTED');
CREATE TYPE period_status_enum AS ENUM ('DRAFT', 'CALCULATING', 'CALCULATED', 'LOCKED', 'PAID');
CREATE TYPE payslip_line_type_enum AS ENUM (
    'BASE_SALARY_PRORATED', 'MEAL_ALLOWANCE', 'FUEL_ALLOWANCE', 'OVERTIME_PAY',
    'BONUS', 'DEDUCTION_BHXH', 'DEDUCTION_BHYT', 'DEDUCTION_BHTN', 'PERSONAL_INCOME_TAX', 'PENALTY'
);

CREATE TABLE departments (
    id BIGSERIAL PRIMARY KEY,
    department_code VARCHAR(50) NOT NULL UNIQUE,
    department_name VARCHAR(150) NOT NULL,
    manager_name VARCHAR(100),
    description TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE employees (
    id BIGSERIAL PRIMARY KEY,
    employee_code VARCHAR(30) NOT NULL UNIQUE,
    full_name VARCHAR(100) NOT NULL,
    date_of_birth DATE,
    gender VARCHAR(10),
    identity_card_number VARCHAR(20) NOT NULL UNIQUE,
    email VARCHAR(120) NOT NULL UNIQUE,
    phone_number VARCHAR(20),
    bank_account_number VARCHAR(50) NOT NULL,
    bank_name VARCHAR(100) NOT NULL,
    department_id BIGINT REFERENCES departments(id) ON DELETE SET NULL,
    hire_date DATE NOT NULL DEFAULT CURRENT_DATE,
    status VARCHAR(30) NOT NULL DEFAULT 'ACTIVE',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE user_accounts (
    id BIGSERIAL PRIMARY KEY,
    employee_id BIGINT UNIQUE REFERENCES employees(id) ON DELETE CASCADE,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role user_role_enum NOT NULL DEFAULT 'ROLE_EMPLOYEE',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    last_login_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE contracts (
    id BIGSERIAL PRIMARY KEY,
    employee_id BIGINT NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
    contract_number VARCHAR(50) NOT NULL UNIQUE,
    contract_type VARCHAR(50) NOT NULL DEFAULT 'FULL_TIME',
    base_salary NUMERIC(15, 2) NOT NULL CHECK (base_salary >= 0),
    meal_allowance NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (meal_allowance >= 0),
    fuel_allowance NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (fuel_allowance >= 0),
    start_date DATE NOT NULL,
    end_date DATE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_contract_dates CHECK (end_date IS NULL OR end_date >= start_date)
);

CREATE TABLE payroll_periods (
    id BIGSERIAL PRIMARY KEY,
    period_code VARCHAR(30) NOT NULL UNIQUE,
    period_month INT NOT NULL CHECK (period_month BETWEEN 1 AND 12),
    period_year INT NOT NULL CHECK (period_year >= 2020),
    standard_work_days NUMERIC(4, 2) NOT NULL DEFAULT 22.00 CHECK (standard_work_days > 0),
    status period_status_enum NOT NULL DEFAULT 'DRAFT',
    total_gross_amount NUMERIC(15, 2) NOT NULL DEFAULT 0.00,
    total_net_amount NUMERIC(15, 2) NOT NULL DEFAULT 0.00,
    locked_at TIMESTAMP WITH TIME ZONE,
    locked_by VARCHAR(100),
    paid_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_period_month_year UNIQUE (period_month, period_year)
);

CREATE TABLE timesheets (
    id BIGSERIAL PRIMARY KEY,
    employee_id BIGINT NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
    period_id BIGINT REFERENCES payroll_periods(id) ON DELETE SET NULL,
    work_date DATE NOT NULL,
    check_in_time TIME,
    check_out_time TIME,
    actual_hours_worked NUMERIC(4, 2) DEFAULT 0.00 CHECK (actual_hours_worked >= 0),
    work_shift_status work_shift_status_enum NOT NULL DEFAULT 'PRESENT',
    work_units NUMERIC(3, 2) NOT NULL DEFAULT 1.00 CHECK (work_units >= 0 AND work_units <= 2.0),
    is_locked BOOLEAN NOT NULL DEFAULT FALSE,
    note VARCHAR(255),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_emp_work_date UNIQUE (employee_id, work_date)
);

CREATE TABLE leave_requests (
    id BIGSERIAL PRIMARY KEY,
    employee_id BIGINT NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
    leave_type leave_type_enum NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    total_days NUMERIC(4, 2) NOT NULL CHECK (total_days > 0),
    reason TEXT NOT NULL,
    approval_status approval_status_enum NOT NULL DEFAULT 'PENDING',
    approved_by VARCHAR(100),
    approved_at TIMESTAMP WITH TIME ZONE,
    reject_reason TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_leave_dates CHECK (end_date >= start_date)
);

CREATE TABLE deduction_rates (
    id BIGSERIAL PRIMARY KEY,
    rate_code VARCHAR(30) NOT NULL UNIQUE,
    rate_name VARCHAR(100) NOT NULL,
    employee_rate NUMERIC(6, 4) NOT NULL CHECK (employee_rate >= 0),
    employer_rate NUMERIC(6, 4) NOT NULL CHECK (employer_rate >= 0),
    effective_from DATE NOT NULL,
    effective_to DATE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT chk_deduction_dates CHECK (effective_to IS NULL OR effective_to >= effective_from)
);

CREATE TABLE payslips (
    id BIGSERIAL PRIMARY KEY,
    period_id BIGINT NOT NULL REFERENCES payroll_periods(id) ON DELETE CASCADE,
    employee_id BIGINT NOT NULL REFERENCES employees(id) ON DELETE CASCADE,
    actual_work_days NUMERIC(5, 2) NOT NULL DEFAULT 0.00 CHECK (actual_work_days >= 0),
    paid_leave_days NUMERIC(5, 2) NOT NULL DEFAULT 0.00 CHECK (paid_leave_days >= 0),
    overtime_hours NUMERIC(5, 2) NOT NULL DEFAULT 0.00 CHECK (overtime_hours >= 0),
    gross_salary NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (gross_salary >= 0),
    total_deductions NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (total_deductions >= 0),
    net_salary NUMERIC(15, 2) NOT NULL DEFAULT 0.00 CHECK (net_salary >= 0),
    note TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_period_employee UNIQUE (period_id, employee_id)
);

CREATE TABLE payslip_lines (
    id BIGSERIAL PRIMARY KEY,
    payslip_id BIGINT NOT NULL REFERENCES payslips(id) ON DELETE CASCADE,
    line_type payslip_line_type_enum NOT NULL,
    line_description VARCHAR(255) NOT NULL,
    amount NUMERIC(15, 2) NOT NULL CHECK (amount >= 0),
    is_deduction BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- INDEXES
CREATE INDEX idx_employees_department ON employees(department_id);
CREATE INDEX idx_contracts_emp_active ON contracts(employee_id, is_active);
CREATE INDEX idx_timesheets_emp_date ON timesheets(employee_id, work_date);
CREATE INDEX idx_payslips_period_emp ON payslips(period_id, employee_id);
CREATE INDEX idx_payslip_lines_payslip ON payslip_lines(payslip_id);
