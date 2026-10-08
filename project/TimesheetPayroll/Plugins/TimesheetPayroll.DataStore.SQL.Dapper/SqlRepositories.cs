using System.Data;
using Dapper;
using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.DataStore.SQL.Dapper;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly IDataAccess _db;
    public EmployeeRepository(IDataAccess db) => _db = db;

    public async Task<IEnumerable<Employee>> GetEmployeesAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                e.id AS Id, e.employee_code AS EmployeeCode, e.full_name AS FullName,
                e.date_of_birth AS DateOfBirth, e.gender AS Gender, e.identity_card_number AS IdentityCardNumber,
                e.email AS Email, e.phone_number AS PhoneNumber, e.bank_account_number AS BankAccountNumber,
                e.bank_name AS BankName, e.department_id AS DepartmentId, d.department_name AS DepartmentName,
                e.hire_date AS HireDate, e.status AS Status,
                ISNULL(c.base_salary, 10000000) AS BaseSalary,
                ISNULL(c.meal_allowance, 500000) AS MealAllowance,
                ISNULL(c.fuel_allowance, 0) AS FuelAllowance
            FROM employees e
            LEFT JOIN departments d ON e.department_id = d.id
            LEFT JOIN contracts c ON e.id = c.employee_id AND c.is_active = 1
            ORDER BY e.id;";
        return await conn.QueryAsync<Employee>(sql);
    }

    public async Task<Employee?> GetEmployeeByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                e.id AS Id, e.employee_code AS EmployeeCode, e.full_name AS FullName,
                e.date_of_birth AS DateOfBirth, e.gender AS Gender, e.identity_card_number AS IdentityCardNumber,
                e.email AS Email, e.phone_number AS PhoneNumber, e.bank_account_number AS BankAccountNumber,
                e.bank_name AS BankName, e.department_id AS DepartmentId, d.department_name AS DepartmentName,
                e.hire_date AS HireDate, e.status AS Status,
                ISNULL(c.base_salary, 10000000) AS BaseSalary,
                ISNULL(c.meal_allowance, 500000) AS MealAllowance,
                ISNULL(c.fuel_allowance, 0) AS FuelAllowance
            FROM employees e
            LEFT JOIN departments d ON e.department_id = d.id
            LEFT JOIN contracts c ON e.id = c.employee_id AND c.is_active = 1
            WHERE e.id = @Id;";
        return await conn.QueryFirstOrDefaultAsync<Employee>(sql, new { Id = id });
    }

    public async Task<Employee?> GetEmployeeByCodeAsync(string code)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                e.id AS Id, e.employee_code AS EmployeeCode, e.full_name AS FullName,
                e.date_of_birth AS DateOfBirth, e.gender AS Gender, e.identity_card_number AS IdentityCardNumber,
                e.email AS Email, e.phone_number AS PhoneNumber, e.bank_account_number AS BankAccountNumber,
                e.bank_name AS BankName, e.department_id AS DepartmentId, d.department_name AS DepartmentName,
                e.hire_date AS HireDate, e.status AS Status,
                ISNULL(c.base_salary, 10000000) AS BaseSalary,
                ISNULL(c.meal_allowance, 500000) AS MealAllowance,
                ISNULL(c.fuel_allowance, 0) AS FuelAllowance
            FROM employees e
            LEFT JOIN departments d ON e.department_id = d.id
            LEFT JOIN contracts c ON e.id = c.employee_id AND c.is_active = 1
            WHERE e.employee_code = @Code;";
        return await conn.QueryFirstOrDefaultAsync<Employee>(sql, new { Code = code });
    }
}

public class TimesheetRepository : ITimesheetRepository
{
    private readonly IDataAccess _db;
    public TimesheetRepository(IDataAccess db) => _db = db;

    public async Task<IEnumerable<Timesheet>> GetTimesheetsByPeriodAsync(long periodId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                t.id AS Id, t.employee_id AS EmployeeId, e.employee_code AS EmployeeCode,
                e.full_name AS EmployeeName, t.period_id AS PeriodId, t.work_date AS WorkDate,
                t.check_in_time AS CheckInTime, t.check_out_time AS CheckOutTime,
                t.actual_hours_worked AS ActualHoursWorked, t.work_shift_status AS WorkShiftStatus,
                t.work_units AS WorkUnits, t.is_locked AS IsLocked, t.note AS Note
            FROM timesheets t
            JOIN employees e ON t.employee_id = e.id
            WHERE t.period_id = @PeriodId
            ORDER BY t.work_date DESC, e.employee_code;";
        return await conn.QueryAsync<Timesheet>(sql, new { PeriodId = periodId });
    }

    public async Task<IEnumerable<Timesheet>> GetTimesheetsByEmployeeAndMonthAsync(long employeeId, int month, int year)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                t.id AS Id, t.employee_id AS EmployeeId, e.employee_code AS EmployeeCode,
                e.full_name AS EmployeeName, t.period_id AS PeriodId, t.work_date AS WorkDate,
                t.check_in_time AS CheckInTime, t.check_out_time AS CheckOutTime,
                t.actual_hours_worked AS ActualHoursWorked, t.work_shift_status AS WorkShiftStatus,
                t.work_units AS WorkUnits, t.is_locked AS IsLocked, t.note AS Note
            FROM timesheets t
            JOIN employees e ON t.employee_id = e.id
            WHERE t.employee_id = @EmployeeId AND MONTH(t.work_date) = @Month AND YEAR(t.work_date) = @Year
            ORDER BY t.work_date ASC;";
        return await conn.QueryAsync<Timesheet>(sql, new { EmployeeId = employeeId, Month = month, Year = year });
    }

    public async Task<decimal> CountWorkDaysByEmployeeAndPeriodAsync(long employeeId, int month, int year)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT ISNULL(SUM(work_units), 0)
            FROM timesheets
            WHERE employee_id = @EmployeeId AND MONTH(work_date) = @Month AND YEAR(work_date) = @Year;";
        return await conn.ExecuteScalarAsync<decimal>(sql, new { EmployeeId = employeeId, Month = month, Year = year });
    }

    public async Task AddOrUpdateTimesheetAsync(Timesheet t)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            MERGE timesheets AS target
            USING (SELECT @EmployeeId AS emp_id, @WorkDate AS wdate) AS source
            ON (target.employee_id = source.emp_id AND target.work_date = source.wdate)
            WHEN MATCHED THEN
                UPDATE SET 
                    check_in_time = @CheckInTime,
                    check_out_time = @CheckOutTime,
                    actual_hours_worked = @ActualHoursWorked,
                    work_shift_status = @WorkShiftStatus,
                    work_units = @WorkUnits,
                    note = @Note,
                    updated_at = GETDATE()
            WHEN NOT MATCHED THEN
                INSERT (employee_id, period_id, work_date, check_in_time, check_out_time, actual_hours_worked, work_shift_status, work_units, is_locked, note)
                VALUES (@EmployeeId, @PeriodId, @WorkDate, @CheckInTime, @CheckOutTime, @ActualHoursWorked, @WorkShiftStatus, @WorkUnits, @IsLocked, @Note);";
        await conn.ExecuteAsync(sql, t);
    }

    public async Task LockTimesheetsByPeriodAsync(long periodId)
    {
        using var conn = _db.CreateConnection();
        const string sql = "UPDATE timesheets SET is_locked = 1, updated_at = GETDATE() WHERE period_id = @PeriodId;";
        await conn.ExecuteAsync(sql, new { PeriodId = periodId });
    }
}

public class PayrollPeriodRepository : IPayrollPeriodRepository
{
    private readonly IDataAccess _db;
    public PayrollPeriodRepository(IDataAccess db) => _db = db;

    public async Task<IEnumerable<PayrollPeriod>> GetPeriodsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                id AS Id, period_code AS PeriodCode, period_month AS PeriodMonth,
                period_year AS PeriodYear, standard_work_days AS StandardWorkDays,
                status AS Status, total_gross_amount AS TotalGrossAmount,
                total_net_amount AS TotalNetAmount, locked_at AS LockedAt,
                locked_by AS LockedBy, paid_at AS PaidAt
            FROM payroll_periods
            ORDER BY period_year DESC, period_month DESC;";
        return await conn.QueryAsync<PayrollPeriod>(sql);
    }

    public async Task<PayrollPeriod?> GetPeriodByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                id AS Id, period_code AS PeriodCode, period_month AS PeriodMonth,
                period_year AS PeriodYear, standard_work_days AS StandardWorkDays,
                status AS Status, total_gross_amount AS TotalGrossAmount,
                total_net_amount AS TotalNetAmount, locked_at AS LockedAt,
                locked_by AS LockedBy, paid_at AS PaidAt
            FROM payroll_periods
            WHERE id = @Id;";
        return await conn.QueryFirstOrDefaultAsync<PayrollPeriod>(sql, new { Id = id });
    }

    public async Task<PayrollPeriod?> GetPeriodByMonthYearAsync(int month, int year)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                id AS Id, period_code AS PeriodCode, period_month AS PeriodMonth,
                period_year AS PeriodYear, standard_work_days AS StandardWorkDays,
                status AS Status, total_gross_amount AS TotalGrossAmount,
                total_net_amount AS TotalNetAmount, locked_at AS LockedAt,
                locked_by AS LockedBy, paid_at AS PaidAt
            FROM payroll_periods
            WHERE period_month = @Month AND period_year = @Year;";
        return await conn.QueryFirstOrDefaultAsync<PayrollPeriod>(sql, new { Month = month, Year = year });
    }

    public async Task UpdatePeriodStatusAsync(long periodId, PeriodStatus status, decimal grossTotal, decimal netTotal)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE payroll_periods
            SET status = @Status,
                total_gross_amount = @GrossTotal,
                total_net_amount = @NetTotal,
                locked_at = CASE WHEN @Status = 'LOCKED' THEN GETDATE() ELSE locked_at END,
                updated_at = GETDATE()
            WHERE id = @PeriodId;";
        await conn.ExecuteAsync(sql, new { PeriodId = periodId, Status = status.ToString(), GrossTotal = grossTotal, NetTotal = netTotal });
    }
}

public class PayslipRepository : IPayslipRepository
{
    private readonly IDataAccess _db;
    public PayslipRepository(IDataAccess db) => _db = db;

    public async Task<IEnumerable<Payslip>> GetPayslipsByPeriodIdAsync(long periodId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                p.id AS Id, p.period_id AS PeriodId, p.employee_id AS EmployeeId,
                e.employee_code AS EmployeeCode, e.full_name AS EmployeeName,
                d.department_name AS DepartmentName, e.bank_account_number AS BankAccountNumber,
                e.bank_name AS BankName, p.actual_work_days AS ActualWorkDays,
                p.paid_leave_days AS PaidLeaveDays, p.overtime_hours AS OvertimeHours,
                p.gross_salary AS GrossSalary, p.total_deductions AS TotalDeductions,
                p.net_salary AS NetSalary, p.note AS Note
            FROM payslips p
            JOIN employees e ON p.employee_id = e.id
            LEFT JOIN departments d ON e.department_id = d.id
            WHERE p.period_id = @PeriodId
            ORDER BY e.employee_code;";

        var payslips = (await conn.QueryAsync<Payslip>(sql, new { PeriodId = periodId })).ToList();

        // Nạp lines
        foreach (var p in payslips)
        {
            const string lineSql = @"
                SELECT id AS Id, payslip_id AS PayslipId, line_type AS LineType,
                       line_description AS LineDescription, amount AS Amount,
                       is_deduction AS IsDeduction
                FROM payslip_lines
                WHERE payslip_id = @PayslipId
                ORDER BY is_deduction ASC, id ASC;";
            p.Lines = (await conn.QueryAsync<PayslipLine>(lineSql, new { PayslipId = p.Id })).ToList();
        }

        return payslips;
    }

    public async Task<Payslip?> GetPayslipByPeriodAndEmployeeAsync(long periodId, long employeeId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                p.id AS Id, p.period_id AS PeriodId, p.employee_id AS EmployeeId,
                e.employee_code AS EmployeeCode, e.full_name AS EmployeeName,
                d.department_name AS DepartmentName, e.bank_account_number AS BankAccountNumber,
                e.bank_name AS BankName, p.actual_work_days AS ActualWorkDays,
                p.paid_leave_days AS PaidLeaveDays, p.overtime_hours AS OvertimeHours,
                p.gross_salary AS GrossSalary, p.total_deductions AS TotalDeductions,
                p.net_salary AS NetSalary, p.note AS Note
            FROM payslips p
            JOIN employees e ON p.employee_id = e.id
            LEFT JOIN departments d ON e.department_id = d.id
            WHERE p.period_id = @PeriodId AND p.employee_id = @EmployeeId;";

        var payslip = await conn.QueryFirstOrDefaultAsync<Payslip>(sql, new { PeriodId = periodId, EmployeeId = employeeId });
        if (payslip != null)
        {
            const string lineSql = @"
                SELECT id AS Id, payslip_id AS PayslipId, line_type AS LineType,
                       line_description AS LineDescription, amount AS Amount,
                       is_deduction AS IsDeduction
                FROM payslip_lines
                WHERE payslip_id = @PayslipId
                ORDER BY is_deduction ASC, id ASC;";
            payslip.Lines = (await conn.QueryAsync<PayslipLine>(lineSql, new { PayslipId = payslip.Id })).ToList();
        }
        return payslip;
    }

    public async Task SavePayslipBatchAsync(IEnumerable<Payslip> payslips)
    {
        using var conn = _db.CreateConnection();
        if (conn.State != ConnectionState.Open) conn.Open();
        using var tx = conn.BeginTransaction();

        try
        {
            foreach (var p in payslips)
            {
                // Kiểm tra hoặc chèn mới
                const string checkSql = "SELECT id FROM payslips WHERE period_id = @PeriodId AND employee_id = @EmployeeId;";
                long? existingId = await conn.ExecuteScalarAsync<long?>(checkSql, new { p.PeriodId, p.EmployeeId }, tx);

                if (existingId.HasValue)
                {
                    p.Id = existingId.Value;
                    const string updateSql = @"
                        UPDATE payslips SET 
                            actual_work_days = @ActualWorkDays,
                            paid_leave_days = @PaidLeaveDays,
                            overtime_hours = @OvertimeHours,
                            gross_salary = @GrossSalary,
                            total_deductions = @TotalDeductions,
                            net_salary = @NetSalary,
                            updated_at = GETDATE()
                        WHERE id = @Id;";
                    await conn.ExecuteAsync(updateSql, p, tx);

                    await conn.ExecuteAsync("DELETE FROM payslip_lines WHERE payslip_id = @Id;", new { p.Id }, tx);
                }
                else
                {
                    const string insertSql = @"
                        INSERT INTO payslips (period_id, employee_id, actual_work_days, paid_leave_days, overtime_hours, gross_salary, total_deductions, net_salary)
                        VALUES (@PeriodId, @EmployeeId, @ActualWorkDays, @PaidLeaveDays, @OvertimeHours, @GrossSalary, @TotalDeductions, @NetSalary);
                        SELECT CAST(SCOPE_IDENTITY() as BIGINT);";
                    p.Id = await conn.ExecuteScalarAsync<long>(insertSql, p, tx);
                }

                foreach (var line in p.Lines)
                {
                    line.PayslipId = p.Id;
                    const string insertLineSql = @"
                        INSERT INTO payslip_lines (payslip_id, line_type, line_description, amount, is_deduction)
                        VALUES (@PayslipId, @LineType, @LineDescription, @Amount, @IsDeduction);";
                    await conn.ExecuteAsync(insertLineSql, new {
                        line.PayslipId,
                        LineType = line.LineType.ToString(),
                        line.LineDescription,
                        line.Amount,
                        line.IsDeduction
                    }, tx);
                }
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}

public class DeductionRateRepository : IDeductionRateRepository
{
    private readonly IDataAccess _db;
    public DeductionRateRepository(IDataAccess db) => _db = db;

    public async Task<IEnumerable<DeductionRate>> GetActiveRatesAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT id AS Id, rate_code AS RateCode, rate_name AS RateName,
                   employee_rate AS EmployeeRate, employer_rate AS EmployerRate,
                   effective_from AS EffectiveFrom, effective_to AS EffectiveTo,
                   is_active AS IsActive
            FROM deduction_rates
            WHERE is_active = 1;";
        return await conn.QueryAsync<DeductionRate>(sql);
    }
}

public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly IDataAccess _db;
    public LeaveRequestRepository(IDataAccess db) => _db = db;

    public async Task<IEnumerable<LeaveRequest>> GetPendingLeaveRequestsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT l.id AS Id, l.employee_id AS EmployeeId, e.full_name AS EmployeeName,
                   l.leave_type AS LeaveType, l.start_date AS StartDate, l.end_date AS EndDate,
                   l.total_days AS TotalDays, l.reason AS Reason, l.approval_status AS ApprovalStatus,
                   l.approved_by AS ApprovedBy, l.approved_at AS ApprovedAt
            FROM leave_requests l
            JOIN employees e ON l.employee_id = e.id
            WHERE l.approval_status = 'PENDING'
            ORDER BY l.start_date ASC;";
        return await conn.QueryAsync<LeaveRequest>(sql);
    }

    public async Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeAsync(long employeeId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT l.id AS Id, l.employee_id AS EmployeeId, e.full_name AS EmployeeName,
                   l.leave_type AS LeaveType, l.start_date AS StartDate, l.end_date AS EndDate,
                   l.total_days AS TotalDays, l.reason AS Reason, l.approval_status AS ApprovalStatus,
                   l.approved_by AS ApprovedBy, l.approved_at AS ApprovedAt
            FROM leave_requests l
            JOIN employees e ON l.employee_id = e.id
            WHERE l.employee_id = @EmployeeId
            ORDER BY l.start_date DESC;";
        return await conn.QueryAsync<LeaveRequest>(sql, new { EmployeeId = employeeId });
    }

    public async Task CreateLeaveRequestAsync(LeaveRequest r)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            INSERT INTO leave_requests (employee_id, leave_type, start_date, end_date, total_days, reason, approval_status)
            VALUES (@EmployeeId, @LeaveType, @StartDate, @EndDate, @TotalDays, @Reason, 'PENDING');";
        await conn.ExecuteAsync(sql, new {
            r.EmployeeId,
            LeaveType = r.LeaveType.ToString(),
            r.StartDate,
            r.EndDate,
            r.TotalDays,
            r.Reason
        });
    }

    public async Task ApproveOrRejectLeaveRequestAsync(long requestId, ApprovalStatus status, string reviewer)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE leave_requests
            SET approval_status = @Status,
                approved_by = @Reviewer,
                approved_at = GETDATE(),
                updated_at = GETDATE()
            WHERE id = @RequestId;";
        await conn.ExecuteAsync(sql, new { RequestId = requestId, Status = status.ToString(), Reviewer = reviewer });
    }
}

public class UserAccountRepository : IUserAccountRepository
{
    private readonly IDataAccess _db;
    public UserAccountRepository(IDataAccess db) => _db = db;

    public async Task<UserAccount?> GetByUsernameAsync(string username)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                u.id AS Id, u.employee_id AS EmployeeId, u.username AS Username, 
                u.password_hash AS PasswordHash, u.role AS Role, u.is_active AS IsActive, 
                u.last_login_at AS LastLoginAt, u.created_at AS CreatedAt, u.updated_at AS UpdatedAt,
                e.full_name AS FullName, e.employee_code AS EmployeeCode, e.email AS Email,
                d.department_name AS DepartmentName
            FROM user_accounts u
            INNER JOIN employees e ON u.employee_id = e.id
            LEFT JOIN departments d ON e.department_id = d.id
            WHERE u.username = @Username;";
        return await conn.QueryFirstOrDefaultAsync<UserAccount>(sql, new { Username = username });
    }

    public async Task<UserAccount?> AuthenticateAsync(string username, string password)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                u.id AS Id, u.employee_id AS EmployeeId, u.username AS Username, 
                u.password_hash AS PasswordHash, u.role AS Role, u.is_active AS IsActive, 
                u.last_login_at AS LastLoginAt, u.created_at AS CreatedAt, u.updated_at AS UpdatedAt,
                e.full_name AS FullName, e.employee_code AS EmployeeCode, e.email AS Email,
                d.department_name AS DepartmentName
            FROM user_accounts u
            INNER JOIN employees e ON u.employee_id = e.id
            LEFT JOIN departments d ON e.department_id = d.id
            WHERE u.username = @Username AND u.is_active = 1;";
        
        var user = await conn.QueryFirstOrDefaultAsync<UserAccount>(sql, new { Username = username });
        if (user == null)
            return null;

        bool isPasswordValid = false;
        try
        {
            if (!string.IsNullOrEmpty(user.PasswordHash) && user.PasswordHash.StartsWith("$2"))
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
        }
        catch
        {
            isPasswordValid = false;
        }

        // Hỗ trợ dự phòng nếu lưu plaintext hoặc demo 123456
        if (!isPasswordValid && (password == user.PasswordHash || password == "123456"))
        {
            isPasswordValid = true;
        }

        return isPasswordValid ? user : null;
    }

    public async Task<IEnumerable<UserAccount>> GetAllUsersAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                u.id AS Id, u.employee_id AS EmployeeId, u.username AS Username, 
                u.password_hash AS PasswordHash, u.role AS Role, u.is_active AS IsActive, 
                u.last_login_at AS LastLoginAt, u.created_at AS CreatedAt, u.updated_at AS UpdatedAt,
                e.full_name AS FullName, e.employee_code AS EmployeeCode, e.email AS Email,
                d.department_name AS DepartmentName
            FROM user_accounts u
            INNER JOIN employees e ON u.employee_id = e.id
            LEFT JOIN departments d ON e.department_id = d.id
            ORDER BY u.id;";
        return await conn.QueryAsync<UserAccount>(sql);
    }

    public async Task UpdateLastLoginAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE user_accounts 
            SET last_login_at = GETDATE(), updated_at = GETDATE() 
            WHERE id = @UserId;";
        await conn.ExecuteAsync(sql, new { UserId = userId });
    }
}

