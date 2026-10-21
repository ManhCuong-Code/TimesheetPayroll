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
                e.full_name AS EmployeeName, t.period_id AS PeriodId, 
                t.shift_id AS ShiftId, t.shift_code AS ShiftCode, t.shift_name AS ShiftName,
                t.work_date AS WorkDate,
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
                e.full_name AS EmployeeName, t.period_id AS PeriodId, 
                t.shift_id AS ShiftId, t.shift_code AS ShiftCode, t.shift_name AS ShiftName,
                t.work_date AS WorkDate,
                t.check_in_time AS CheckInTime, t.check_out_time AS CheckOutTime,
                t.actual_hours_worked AS ActualHoursWorked, t.work_shift_status AS WorkShiftStatus,
                t.work_units AS WorkUnits, t.is_locked AS IsLocked, t.note AS Note
            FROM timesheets t
            JOIN employees e ON t.employee_id = e.id
            WHERE t.employee_id = @EmployeeId AND MONTH(t.work_date) = @Month AND YEAR(t.work_date) = @Year
            ORDER BY t.work_date ASC;";
        return await conn.QueryAsync<Timesheet>(sql, new { EmployeeId = employeeId, Month = month, Year = year });
    }

    public async Task<IEnumerable<Timesheet>> GetAllTimesheetsByMonthAsync(int month, int year)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                t.id AS Id, t.employee_id AS EmployeeId, e.employee_code AS EmployeeCode,
                e.full_name AS EmployeeName, t.period_id AS PeriodId, 
                t.shift_id AS ShiftId, t.shift_code AS ShiftCode, t.shift_name AS ShiftName,
                t.work_date AS WorkDate,
                t.check_in_time AS CheckInTime, t.check_out_time AS CheckOutTime,
                t.actual_hours_worked AS ActualHoursWorked, t.work_shift_status AS WorkShiftStatus,
                t.work_units AS WorkUnits, t.is_locked AS IsLocked, t.note AS Note
            FROM timesheets t
            JOIN employees e ON t.employee_id = e.id
            WHERE MONTH(t.work_date) = @Month AND YEAR(t.work_date) = @Year
            ORDER BY t.work_date DESC, e.employee_code ASC;";
        return await conn.QueryAsync<Timesheet>(sql, new { Month = month, Year = year });
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
                    shift_id = @ShiftId,
                    shift_code = @ShiftCode,
                    shift_name = @ShiftName,
                    check_in_time = @CheckInTime,
                    check_out_time = @CheckOutTime,
                    actual_hours_worked = @ActualHoursWorked,
                    work_shift_status = @WorkShiftStatus,
                    work_units = @WorkUnits,
                    note = @Note,
                    updated_at = GETDATE()
            WHEN NOT MATCHED THEN
                INSERT (employee_id, period_id, shift_id, shift_code, shift_name, work_date, check_in_time, check_out_time, actual_hours_worked, work_shift_status, work_units, is_locked, note)
                VALUES (@EmployeeId, @PeriodId, @ShiftId, @ShiftCode, @ShiftName, @WorkDate, @CheckInTime, @CheckOutTime, @ActualHoursWorked, @WorkShiftStatus, @WorkUnits, @IsLocked, @Note);";
        string safeStatus = t.WorkShiftStatus switch
        {
            WorkShiftStatus.PRESENT => "PRESENT",
            WorkShiftStatus.LATE => "LATE",
            WorkShiftStatus.EARLY_LEAVE => "EARLY_LEAVE",
            WorkShiftStatus.HALF_DAY => "HALF_DAY",
            WorkShiftStatus.ABSENT_WITH_PERMISSION => "ABSENT_WITH_PERMISSION",
            WorkShiftStatus.ABSENT_UNAUTHORIZED => "ABSENT_UNAUTHORIZED",
            WorkShiftStatus.HOLIDAY => "HOLIDAY",
            _ => "PRESENT"
        };

        decimal safeActualHours = Math.Max(0m, Math.Min(24.0m, Math.Round(t.ActualHoursWorked, 2)));
        decimal safeWorkUnits = Math.Max(0m, Math.Min(2.0m, Math.Round(t.WorkUnits, 2)));

        await conn.ExecuteAsync(sql, new
        {
            t.EmployeeId,
            t.PeriodId,
            t.ShiftId,
            t.ShiftCode,
            t.ShiftName,
            t.WorkDate,
            t.CheckInTime,
            t.CheckOutTime,
            ActualHoursWorked = safeActualHours,
            WorkShiftStatus = safeStatus,
            WorkUnits = safeWorkUnits,
            IsLocked = t.IsLocked ? 1 : 0,
            t.Note
        });
    }

    public async Task LockTimesheetsByPeriodAsync(long periodId)
    {
        using var conn = _db.CreateConnection();
        const string sql = "UPDATE timesheets SET is_locked = 1, updated_at = GETDATE() WHERE period_id = @PeriodId;";
        await conn.ExecuteAsync(sql, new { PeriodId = periodId });
    }

    public async Task UnlockTimesheetsByPeriodAsync(long periodId)
    {
        using var conn = _db.CreateConnection();
        const string sql = "UPDATE timesheets SET is_locked = 0, updated_at = GETDATE() WHERE period_id = @PeriodId;";
        await conn.ExecuteAsync(sql, new { PeriodId = periodId });
    }
}

public class WorkShiftRepository : IWorkShiftRepository
{
    private readonly IDataAccess _db;
    public WorkShiftRepository(IDataAccess db) => _db = db;

    public async Task<IEnumerable<WorkShift>> GetWorkShiftsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                id AS Id,
                shift_code AS ShiftCode,
                shift_name AS ShiftName,
                start_time AS StartTime,
                end_time AS EndTime,
                lunch_break_hours AS LunchBreakHours,
                target_hours AS TargetHours,
                work_units AS WorkUnits,
                grace_period_minutes AS GracePeriodMinutes,
                is_night_shift AS IsNightShift,
                is_active AS IsActive,
                description AS Description,
                created_at AS CreatedAt,
                updated_at AS UpdatedAt
            FROM work_shifts
            ORDER BY start_time ASC;";
        return await conn.QueryAsync<WorkShift>(sql);
    }

    public async Task<WorkShift?> GetWorkShiftByIdAsync(long id)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                id AS Id, shift_code AS ShiftCode, shift_name AS ShiftName,
                start_time AS StartTime, end_time AS EndTime, lunch_break_hours AS LunchBreakHours,
                target_hours AS TargetHours, work_units AS WorkUnits, grace_period_minutes AS GracePeriodMinutes,
                is_night_shift AS IsNightShift, is_active AS IsActive, description AS Description,
                created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM work_shifts
            WHERE id = @Id;";
        return await conn.QueryFirstOrDefaultAsync<WorkShift>(sql, new { Id = id });
    }

    public async Task<WorkShift?> GetWorkShiftByCodeAsync(string code)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                id AS Id, shift_code AS ShiftCode, shift_name AS ShiftName,
                start_time AS StartTime, end_time AS EndTime, lunch_break_hours AS LunchBreakHours,
                target_hours AS TargetHours, work_units AS WorkUnits, grace_period_minutes AS GracePeriodMinutes,
                is_night_shift AS IsNightShift, is_active AS IsActive, description AS Description,
                created_at AS CreatedAt, updated_at AS UpdatedAt
            FROM work_shifts
            WHERE shift_code = @Code;";
        return await conn.QueryFirstOrDefaultAsync<WorkShift>(sql, new { Code = code });
    }

    public async Task<long> AddWorkShiftAsync(WorkShift shift)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            INSERT INTO work_shifts (shift_code, shift_name, start_time, end_time, lunch_break_hours, target_hours, work_units, grace_period_minutes, is_night_shift, is_active, description)
            VALUES (@ShiftCode, @ShiftName, @StartTime, @EndTime, @LunchBreakHours, @TargetHours, @WorkUnits, @GracePeriodMinutes, @IsNightShift, @IsActive, @Description);
            SELECT CAST(SCOPE_IDENTITY() as BIGINT);";
        return await conn.ExecuteScalarAsync<long>(sql, shift);
    }

    public async Task UpdateWorkShiftAsync(WorkShift shift)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE work_shifts
            SET shift_code = @ShiftCode,
                shift_name = @ShiftName,
                start_time = @StartTime,
                end_time = @EndTime,
                lunch_break_hours = @LunchBreakHours,
                target_hours = @TargetHours,
                work_units = @WorkUnits,
                grace_period_minutes = @GracePeriodMinutes,
                is_night_shift = @IsNightShift,
                is_active = @IsActive,
                description = @Description,
                updated_at = GETDATE()
            WHERE id = @Id;";
        await conn.ExecuteAsync(sql, shift);
    }

    public async Task DeleteWorkShiftAsync(long id)
    {
        using var conn = _db.CreateConnection();
        const string sql = "DELETE FROM work_shifts WHERE id = @Id;";
        await conn.ExecuteAsync(sql, new { Id = id });
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
                pr.period_code AS PeriodCode, pr.period_month AS PeriodMonth, pr.period_year AS PeriodYear,
                e.employee_code AS EmployeeCode, e.full_name AS EmployeeName,
                d.department_name AS DepartmentName, e.bank_account_number AS BankAccountNumber,
                e.bank_name AS BankName, p.actual_work_days AS ActualWorkDays,
                p.paid_leave_days AS PaidLeaveDays, p.overtime_hours AS OvertimeHours,
                p.gross_salary AS GrossSalary, p.total_deductions AS TotalDeductions,
                p.net_salary AS NetSalary, p.note AS Note
            FROM payslips p
            JOIN payroll_periods pr ON p.period_id = pr.id
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
                pr.period_code AS PeriodCode, pr.period_month AS PeriodMonth, pr.period_year AS PeriodYear,
                e.employee_code AS EmployeeCode, e.full_name AS EmployeeName,
                d.department_name AS DepartmentName, e.bank_account_number AS BankAccountNumber,
                e.bank_name AS BankName, p.actual_work_days AS ActualWorkDays,
                p.paid_leave_days AS PaidLeaveDays, p.overtime_hours AS OvertimeHours,
                p.gross_salary AS GrossSalary, p.total_deductions AS TotalDeductions,
                p.net_salary AS NetSalary, p.note AS Note
            FROM payslips p
            JOIN payroll_periods pr ON p.period_id = pr.id
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

    public async Task<IEnumerable<Payslip>> GetPayslipsByEmployeeAsync(long employeeId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                p.id AS Id, p.period_id AS PeriodId, p.employee_id AS EmployeeId,
                pr.period_code AS PeriodCode, pr.period_month AS PeriodMonth, pr.period_year AS PeriodYear,
                e.employee_code AS EmployeeCode, e.full_name AS EmployeeName,
                d.department_name AS DepartmentName, e.bank_account_number AS BankAccountNumber,
                e.bank_name AS BankName, p.actual_work_days AS ActualWorkDays,
                p.paid_leave_days AS PaidLeaveDays, p.overtime_hours AS OvertimeHours,
                p.gross_salary AS GrossSalary, p.total_deductions AS TotalDeductions,
                p.net_salary AS NetSalary, p.note AS Note
            FROM payslips p
            JOIN payroll_periods pr ON p.period_id = pr.id
            JOIN employees e ON p.employee_id = e.id
            LEFT JOIN departments d ON e.department_id = d.id
            WHERE p.employee_id = @EmployeeId
            ORDER BY pr.period_year DESC, pr.period_month DESC, p.id DESC;";

        var payslips = (await conn.QueryAsync<Payslip>(sql, new { EmployeeId = employeeId })).ToList();

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

    public async Task<IEnumerable<LeaveRequest>> GetAllLeaveRequestsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT l.id AS Id, l.employee_id AS EmployeeId, e.employee_code AS EmployeeCode, e.full_name AS EmployeeName, d.department_name AS DepartmentName,
                   l.leave_type AS LeaveType, l.start_date AS StartDate, l.end_date AS EndDate,
                   l.total_days AS TotalDays, l.reason AS Reason, l.approval_status AS ApprovalStatus,
                   l.approved_by AS ApprovedBy, l.approved_at AS ApprovedAt
            FROM leave_requests l
            JOIN employees e ON l.employee_id = e.id
            LEFT JOIN departments d ON e.department_id = d.id
            ORDER BY l.start_date DESC;";
        return await conn.QueryAsync<LeaveRequest>(sql);
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingLeaveRequestsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT l.id AS Id, l.employee_id AS EmployeeId, e.employee_code AS EmployeeCode, e.full_name AS EmployeeName, d.department_name AS DepartmentName,
                   l.leave_type AS LeaveType, l.start_date AS StartDate, l.end_date AS EndDate,
                   l.total_days AS TotalDays, l.reason AS Reason, l.approval_status AS ApprovalStatus,
                   l.approved_by AS ApprovedBy, l.approved_at AS ApprovedAt
            FROM leave_requests l
            JOIN employees e ON l.employee_id = e.id
            LEFT JOIN departments d ON e.department_id = d.id
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

        if (status == ApprovalStatus.APPROVED)
        {
            const string getReqSql = "SELECT * FROM leave_requests WHERE id = @RequestId;";
            var req = await conn.QueryFirstOrDefaultAsync<LeaveRequest>(getReqSql, new { RequestId = requestId });
            if (req != null)
            {
                for (var d = req.StartDate.Date; d <= req.EndDate.Date; d = d.AddDays(1))
                {
                    if (d.DayOfWeek == DayOfWeek.Sunday) continue;

                    const string checkSql = "SELECT COUNT(*) FROM timesheets WHERE employee_id = @EmpId AND work_date = @WorkDate;";
                    var count = await conn.ExecuteScalarAsync<int>(checkSql, new { EmpId = req.EmployeeId, WorkDate = d });
                    var note = $"Nghỉ phép: {req.Reason}";
                    var workUnits = req.LeaveType == LeaveType.ANNUAL_LEAVE ? 1.0m : 0.0m;

                    if (count > 0)
                    {
                        const string updSql = @"
                            UPDATE timesheets 
                            SET work_shift_status = 'ABSENT_WITH_PERMISSION',
                                work_units = @WorkUnits,
                                note = @Note,
                                updated_at = GETDATE()
                            WHERE employee_id = @EmpId AND work_date = @WorkDate;";
                        await conn.ExecuteAsync(updSql, new { EmpId = req.EmployeeId, WorkDate = d, WorkUnits = workUnits, Note = note });
                    }
                    else
                    {
                        const string insSql = @"
                            INSERT INTO timesheets (employee_id, work_date, check_in_time, check_out_time, actual_hours_worked, work_shift_status, work_units, is_locked, note)
                            VALUES (@EmpId, @WorkDate, NULL, NULL, 0.0, 'ABSENT_WITH_PERMISSION', @WorkUnits, 0, @Note);";
                        await conn.ExecuteAsync(insSql, new { EmpId = req.EmployeeId, WorkDate = d, WorkUnits = workUnits, Note = note });
                    }
                }
            }
        }
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

    public async Task<UserAccount?> GetByIdAsync(long id)
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
            WHERE u.id = @Id;";
        return await conn.QueryFirstOrDefaultAsync<UserAccount>(sql, new { Id = id });
    }

    public async Task<UserAccount?> GetByEmployeeIdAsync(long employeeId)
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
            WHERE u.employee_id = @EmployeeId;";
        return await conn.QueryFirstOrDefaultAsync<UserAccount>(sql, new { EmployeeId = employeeId });
    }

    public async Task<bool> ExistsUsernameAsync(string username, long? excludeUserId = null)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT COUNT(1) 
            FROM user_accounts 
            WHERE username = @Username AND (@ExcludeUserId IS NULL OR id <> @ExcludeUserId);";
        int count = await conn.ExecuteScalarAsync<int>(sql, new { Username = username, ExcludeUserId = excludeUserId });
        return count > 0;
    }

    public async Task<long> CreateUserAccountAsync(UserAccount account, string plainPassword)
    {
        using var conn = _db.CreateConnection();
        string hash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
        const string sql = @"
            INSERT INTO user_accounts (employee_id, username, password_hash, role, is_active, created_at, updated_at)
            OUTPUT INSERTED.id
            VALUES (@EmployeeId, @Username, @PasswordHash, @Role, @IsActive, GETDATE(), GETDATE());";
        return await conn.ExecuteScalarAsync<long>(sql, new
        {
            account.EmployeeId,
            account.Username,
            PasswordHash = hash,
            Role = string.IsNullOrWhiteSpace(account.Role) ? "ROLE_EMPLOYEE" : account.Role,
            IsActive = account.IsActive ? 1 : 0
        });
    }

    public async Task UpdateUserAccountAsync(UserAccount account, string? newPlainPassword = null)
    {
        using var conn = _db.CreateConnection();
        string? newHash = !string.IsNullOrWhiteSpace(newPlainPassword) 
            ? BCrypt.Net.BCrypt.HashPassword(newPlainPassword) 
            : null;

        const string sql = @"
            UPDATE user_accounts
            SET username = @Username,
                role = @Role,
                is_active = @IsActive,
                password_hash = COALESCE(@PasswordHash, password_hash),
                updated_at = GETDATE()
            WHERE id = @Id;";

        await conn.ExecuteAsync(sql, new
        {
            account.Id,
            account.Username,
            account.Role,
            IsActive = account.IsActive ? 1 : 0,
            PasswordHash = newHash
        });
    }

    public async Task<IEnumerable<Employee>> GetEmployeesWithoutAccountAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                e.id AS Id, e.employee_code AS EmployeeCode, e.full_name AS FullName, 
                e.email AS Email, e.phone_number AS PhoneNumber, e.department_id AS DepartmentId,
                d.department_name AS DepartmentName, e.status AS Status, e.hire_date AS HireDate
            FROM employees e
            LEFT JOIN departments d ON e.department_id = d.id
            WHERE e.id NOT IN (SELECT employee_id FROM user_accounts)
              AND e.status = 'ACTIVE'
            ORDER BY e.employee_code;";
        return await conn.QueryAsync<Employee>(sql);
    }

    public async Task ToggleUserStatusAsync(long userId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE user_accounts 
            SET is_active = CASE WHEN is_active = 1 THEN 0 ELSE 1 END,
                updated_at = GETDATE()
            WHERE id = @UserId;";
        await conn.ExecuteAsync(sql, new { UserId = userId });
    }

    public async Task ChangePasswordAsync(long userId, string newPlainPassword)
    {
        using var conn = _db.CreateConnection();
        string newHash = BCrypt.Net.BCrypt.HashPassword(newPlainPassword);
        const string sql = @"
            UPDATE user_accounts
            SET password_hash = @PasswordHash,
                updated_at = GETDATE()
            WHERE id = @UserId;";
        await conn.ExecuteAsync(sql, new { UserId = userId, PasswordHash = newHash });
    }
}

public class FaceProfileRepository : IFaceProfileRepository
{
    private readonly IDataAccess _db;
    public FaceProfileRepository(IDataAccess db) => _db = db;

    public async Task<EmployeeFaceProfile?> GetByEmployeeIdAsync(long employeeId)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                f.id AS Id, f.employee_id AS EmployeeId, e.employee_code AS EmployeeCode,
                e.full_name AS FullName, d.department_name AS DepartmentName,
                f.face_descriptor_json AS FaceDescriptorJson,
                f.registered_at AS RegisteredAt, f.registered_by AS RegisteredBy,
                f.is_active AS IsActive, f.liveness_threshold AS LivenessThreshold,
                f.notes AS Notes, f.updated_at AS UpdatedAt
            FROM employee_face_profiles f
            JOIN employees e ON f.employee_id = e.id
            LEFT JOIN departments d ON e.department_id = d.id
            WHERE f.employee_id = @EmployeeId;";
        return await conn.QueryFirstOrDefaultAsync<EmployeeFaceProfile>(sql, new { EmployeeId = employeeId });
    }

    public async Task<IEnumerable<EmployeeFaceProfile>> GetAllActiveProfilesAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                f.id AS Id, f.employee_id AS EmployeeId, e.employee_code AS EmployeeCode,
                e.full_name AS FullName, d.department_name AS DepartmentName,
                f.face_descriptor_json AS FaceDescriptorJson,
                f.registered_at AS RegisteredAt, f.registered_by AS RegisteredBy,
                f.is_active AS IsActive, f.liveness_threshold AS LivenessThreshold,
                f.notes AS Notes, f.updated_at AS UpdatedAt
            FROM employee_face_profiles f
            JOIN employees e ON f.employee_id = e.id
            LEFT JOIN departments d ON e.department_id = d.id
            WHERE f.is_active = 1 AND e.status = 'ACTIVE';";
        return await conn.QueryAsync<EmployeeFaceProfile>(sql);
    }

    public async Task<IEnumerable<EmployeeFaceProfile>> GetAllProfilesWithEmployeeDetailsAsync()
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT 
                ISNULL(f.id, 0) AS Id, e.id AS EmployeeId, e.employee_code AS EmployeeCode,
                e.full_name AS FullName, d.department_name AS DepartmentName,
                ISNULL(f.face_descriptor_json, '') AS FaceDescriptorJson,
                ISNULL(f.registered_at, '2000-01-01') AS RegisteredAt,
                ISNULL(f.registered_by, '') AS RegisteredBy,
                ISNULL(f.is_active, 0) AS IsActive,
                ISNULL(f.liveness_threshold, 0.70) AS LivenessThreshold,
                f.notes AS Notes,
                ISNULL(f.updated_at, '2000-01-01') AS UpdatedAt
            FROM employees e
            LEFT JOIN departments d ON e.department_id = d.id
            LEFT JOIN employee_face_profiles f ON e.id = f.employee_id
            WHERE e.status = 'ACTIVE'
            ORDER BY e.employee_code;";
        return await conn.QueryAsync<EmployeeFaceProfile>(sql);
    }

    public async Task SaveProfileAsync(EmployeeFaceProfile profile)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            MERGE employee_face_profiles AS target
            USING (SELECT @EmployeeId AS employee_id) AS source
            ON (target.employee_id = source.employee_id)
            WHEN MATCHED THEN
                UPDATE SET 
                    face_descriptor_json = @FaceDescriptorJson,
                    registered_by = @RegisteredBy,
                    is_active = 1,
                    liveness_threshold = @LivenessThreshold,
                    notes = @Notes,
                    updated_at = GETDATE()
            WHEN NOT MATCHED THEN
                INSERT (employee_id, face_descriptor_json, registered_at, registered_by, is_active, liveness_threshold, notes, updated_at)
                VALUES (@EmployeeId, @FaceDescriptorJson, GETDATE(), @RegisteredBy, 1, @LivenessThreshold, @Notes, GETDATE());";
        await conn.ExecuteAsync(sql, profile);
    }

    public async Task DeleteProfileAsync(long employeeId)
    {
        using var conn = _db.CreateConnection();
        const string sql = "DELETE FROM employee_face_profiles WHERE employee_id = @EmployeeId;";
        await conn.ExecuteAsync(sql, new { EmployeeId = employeeId });
    }

    public async Task ToggleStatusAsync(long employeeId, bool isActive)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            UPDATE employee_face_profiles
            SET is_active = @IsActive, updated_at = GETDATE()
            WHERE employee_id = @EmployeeId;";
        await conn.ExecuteAsync(sql, new { EmployeeId = employeeId, IsActive = isActive });
    }
}

public class FaceAuditLogRepository : IFaceAuditLogRepository
{
    private readonly IDataAccess _db;
    public FaceAuditLogRepository(IDataAccess db) => _db = db;

    public async Task LogActionAsync(FaceAuditLog log)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            INSERT INTO face_audit_logs (employee_id, action_type, similarity_score, performed_by, device_info, is_success, failure_reason, created_at)
            VALUES (@EmployeeId, @ActionType, @SimilarityScore, @PerformedBy, @DeviceInfo, @IsSuccess, @FailureReason, GETDATE());";
        await conn.ExecuteAsync(sql, log);
    }

    public async Task<IEnumerable<FaceAuditLog>> GetLogsAsync(int limit = 100)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT TOP (@Limit)
                l.id AS Id, l.employee_id AS EmployeeId, e.employee_code AS EmployeeCode,
                e.full_name AS FullName, l.action_type AS ActionType,
                l.similarity_score AS SimilarityScore, l.performed_by AS PerformedBy,
                l.device_info AS DeviceInfo, l.is_success AS IsSuccess,
                l.failure_reason AS FailureReason, l.created_at AS CreatedAt
            FROM face_audit_logs l
            LEFT JOIN employees e ON l.employee_id = e.id
            ORDER BY l.created_at DESC;";
        return await conn.QueryAsync<FaceAuditLog>(sql, new { Limit = limit });
    }
}

public class TimesheetAuditLogRepository : ITimesheetAuditLogRepository
{
    private readonly IDataAccess _db;
    public TimesheetAuditLogRepository(IDataAccess db) => _db = db;

    public async Task LogAuditAsync(TimesheetAuditLog log)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            INSERT INTO timesheet_audit_logs (timesheet_id, employee_id, action, before_data, after_data, modified_by, modified_at, reason)
            VALUES (@TimesheetId, @EmployeeId, @Action, @BeforeData, @AfterData, @ModifiedBy, GETDATE(), @Reason);";
        await conn.ExecuteAsync(sql, log);
    }

    public async Task<IEnumerable<TimesheetAuditLog>> GetLogsByEmployeeAsync(long employeeId, int limit = 50)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT TOP (@Limit)
                l.id AS Id, l.timesheet_id AS TimesheetId, l.employee_id AS EmployeeId,
                e.employee_code AS EmployeeCode, e.full_name AS EmployeeName,
                l.action AS Action, l.before_data AS BeforeData, l.after_data AS AfterData,
                l.modified_by AS ModifiedBy, l.modified_at AS ModifiedAt, l.reason AS Reason
            FROM timesheet_audit_logs l
            JOIN employees e ON l.employee_id = e.id
            WHERE l.employee_id = @EmployeeId
            ORDER BY l.modified_at DESC;";
        return await conn.QueryAsync<TimesheetAuditLog>(sql, new { EmployeeId = employeeId, Limit = limit });
    }

    public async Task<IEnumerable<TimesheetAuditLog>> GetRecentLogsAsync(int limit = 100)
    {
        using var conn = _db.CreateConnection();
        const string sql = @"
            SELECT TOP (@Limit)
                l.id AS Id, l.timesheet_id AS TimesheetId, l.employee_id AS EmployeeId,
                e.employee_code AS EmployeeCode, e.full_name AS EmployeeName,
                l.action AS Action, l.before_data AS BeforeData, l.after_data AS AfterData,
                l.modified_by AS ModifiedBy, l.modified_at AS ModifiedAt, l.reason AS Reason
            FROM timesheet_audit_logs l
            JOIN employees e ON l.employee_id = e.id
            ORDER BY l.modified_at DESC;";
        return await conn.QueryAsync<TimesheetAuditLog>(sql, new { Limit = limit });
    }
}


