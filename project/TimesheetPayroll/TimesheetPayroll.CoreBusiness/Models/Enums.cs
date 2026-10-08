namespace TimesheetPayroll.CoreBusiness.Models;

public enum UserRole
{
    ROLE_EMPLOYEE,
    ROLE_HR_ACCOUNTANT,
    ROLE_ADMIN
}

public enum WorkShiftStatus
{
    PRESENT,
    LATE,
    EARLY_LEAVE,
    HALF_DAY,
    ABSENT_WITH_PERMISSION,
    ABSENT_UNAUTHORIZED,
    HOLIDAY
}

public enum LeaveType
{
    ANNUAL_LEAVE,
    SICK_LEAVE,
    MATERNITY_LEAVE,
    UNPAID_LEAVE
}

public enum ApprovalStatus
{
    PENDING,
    APPROVED,
    REJECTED
}

public enum PeriodStatus
{
    DRAFT,
    CALCULATING,
    CALCULATED,
    LOCKED,
    PAID
}

public enum PayslipLineType
{
    BASE_SALARY_PRORATED,
    MEAL_ALLOWANCE,
    FUEL_ALLOWANCE,
    OVERTIME_PAY,
    BONUS,
    DEDUCTION_BHXH,
    DEDUCTION_BHYT,
    DEDUCTION_BHTN,
    PERSONAL_INCOME_TAX,
    PENALTY
}
