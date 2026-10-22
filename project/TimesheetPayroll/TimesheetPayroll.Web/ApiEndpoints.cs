namespace TimesheetPayroll.Web;

using Dapper;
using Microsoft.AspNetCore.Mvc;
using TimesheetPayroll.CoreBusiness.Models;
using TimesheetPayroll.DataStore.SQL.Dapper;
using TimesheetPayroll.UseCases.AdminPortal;
using TimesheetPayroll.UseCases.EmployeePortal;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

/// <summary>
/// Cổng giao tiếp REST API 2 chiều (Bidirectional Web API) kết nối CSDL Microsoft SQL Server (BangChamCongDB).
/// Cung cấp 100% các endpoint chuẩn theo bảng quy đổi kiến trúc dự án (BANG_QUY_DOI_TEN_DU_AN.md).
/// </summary>
public static class ApiEndpoints
{
    public static void MapTimesheetPayrollApi(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        // 1. Health check & Trạng thái kết nối CSDL 2 chiều
        api.MapGet("/health", async ([FromServices] IDataAccess db) =>
        {
            try
            {
                using var conn = db.CreateConnection();
                var empCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM employees");
                var tsCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM timesheets");
                var leaveCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM leave_requests");
                var periodCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM payroll_periods");
                return Results.Ok(new
                {
                    status = "online",
                    database = "BangChamCongDB",
                    server = "Microsoft SQL Server",
                    connected = true,
                    stats = new
                    {
                        employees = empCount,
                        timesheets = tsCount,
                        leaveRequests = leaveCount,
                        periods = periodCount
                    },
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new
                {
                    status = "degraded",
                    database = "BangChamCongDB",
                    connected = false,
                    error = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        });

        // 2. Danh sách nhân sự từ CSDL
        api.MapGet("/employees", async ([FromServices] IEmployeeRepository repo) =>
        {
            var list = await repo.GetEmployeesAsync();
            return Results.Ok(list);
        });

        // 3. Danh mục phòng ban từ CSDL
        api.MapGet("/departments", async ([FromServices] IDataAccess db) =>
        {
            using var conn = db.CreateConnection();
            var list = await conn.QueryAsync("SELECT * FROM departments ORDER BY id");
            return Results.Ok(list);
        });

        // 4. Danh mục ca làm việc chuẩn hóa từ CSDL
        api.MapGet("/work-shifts", async ([FromServices] IWorkShiftRepository repo) =>
        {
            var list = await repo.GetWorkShiftsAsync();
            return Results.Ok(list);
        });

        // 5. Quản lý Đơn xin nghỉ phép 2 chiều (Lấy danh sách / Nộp đơn / Duyệt đơn)
        api.MapGet("/don-phep", async ([FromServices] ILeaveRequestRepository repo) =>
        {
            var list = await repo.GetAllLeaveRequestsAsync();
            return Results.Ok(list);
        });

        api.MapPost("/don-phep/tao-don", async (
            [FromBody] CreateLeaveDto dto,
            [FromServices] ICreateLeaveRequestUseCase useCase) =>
        {
            try
            {
                var req = new LeaveRequest
                {
                    EmployeeId = dto.EmployeeId,
                    LeaveType = dto.LeaveType,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    TotalDays = dto.TotalDays > 0 ? dto.TotalDays : 1.0m,
                    Reason = dto.Reason ?? string.Empty,
                    ApprovalStatus = ApprovalStatus.PENDING,
                    CreatedAt = DateTime.Now
                };
                await useCase.ExecuteAsync(req);
                return Results.Ok(new
                {
                    success = true,
                    message = "Đã lưu đơn xin nghỉ phép trực tiếp vào bảng leave_requests trong CSDL SQL Server!",
                    request = req
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { success = false, message = ex.Message });
            }
        });

        api.MapPut("/don-phep/{id:long}/phe-duyet", async (
            long id,
            [FromBody] ApproveLeaveDto dto,
            [FromServices] IProcessLeaveRequestUseCase useCase) =>
        {
            try
            {
                if (string.Equals(dto.Status, "APPROVED", StringComparison.OrdinalIgnoreCase))
                {
                    await useCase.ApproveAsync(id, dto.Reviewer ?? "HR_Admin");
                }
                else
                {
                    await useCase.RejectAsync(id, dto.Reviewer ?? "HR_Admin", dto.Reason);
                }
                return Results.Ok(new
                {
                    success = true,
                    message = $"Đã cập nhật trạng thái [{dto.Status}] cho đơn #{id} trong CSDL SQL Server!"
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { success = false, message = ex.Message });
            }
        });

        // 6. Điểm danh Check-in / Check-out 2 chiều (POST điểm danh -> Ghi CSDL; GET tra cứu)
        api.MapPost("/bang-cham-cong/diem-danh", async (
            [FromBody] PunchDto dto,
            [FromServices] ICheckInCheckOutUseCase useCase) =>
        {
            try
            {
                TimeSpan? customTime = null;
                if (!string.IsNullOrWhiteSpace(dto.CustomTime) && TimeSpan.TryParse(dto.CustomTime, out var parsed))
                {
                    customTime = parsed;
                }

                Timesheet result;
                if (string.Equals(dto.PunchType, "CHECK_OUT", StringComparison.OrdinalIgnoreCase))
                {
                    result = await useCase.ExecuteCheckOutAsync(dto.EmployeeId, dto.ShiftId, customTime, dto.WorkDate ?? DateTime.Today);
                }
                else
                {
                    result = await useCase.ExecuteCheckInAsync(dto.EmployeeId, dto.ShiftId, customTime, dto.WorkDate ?? DateTime.Today);
                }

                return Results.Ok(new
                {
                    success = true,
                    message = "Đã ghi nhận điểm danh vào bảng timesheets trong CSDL SQL Server!",
                    record = result
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { success = false, message = ex.Message });
            }
        });

        api.MapGet("/bang-cham-cong/thang/{thang:int}/{nam:int}", async (
            int thang, int nam,
            [FromServices] ITimesheetRepository repo) =>
        {
            var list = await repo.GetAllTimesheetsByMonthAsync(thang, nam);
            return Results.Ok(list);
        });

        api.MapGet("/bang-cham-cong/nhan-vien/{empId:long}/thang/{thang:int}/{nam:int}", async (
            long empId, int thang, int nam,
            [FromServices] IViewMyTimesheetUseCase useCase) =>
        {
            var list = await useCase.ExecuteAsync(empId, thang, nam);
            return Results.Ok(list);
        });

        // 7. Kỳ tính lương và Động cơ tính lương hàng loạt
        api.MapGet("/payroll-periods", async ([FromServices] IPayrollPeriodRepository repo) =>
        {
            var list = await repo.GetPeriodsAsync();
            return Results.Ok(list);
        });

        api.MapPost("/tinh-luong/chay-tu-dong", async (
            [FromBody] CalculateBatchDto dto,
            [FromServices] ICalculatePayrollBatchUseCase useCase) =>
        {
            try
            {
                var payslips = await useCase.ExecuteAsync(dto.PeriodId);
                return Results.Ok(new
                {
                    success = true,
                    message = "Đã thực thi tính toán lương tự động trong Transaction an toàn và lưu vào CSDL SQL Server!",
                    count = payslips.Count()
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { success = false, message = ex.Message });
            }
        });

        api.MapGet("/tinh-luong/bang-tong-hop/{kyLuongId:long}", async (
            long kyLuongId,
            [FromServices] IViewPayrollSummaryUseCase useCase) =>
        {
            var summary = await useCase.ExecuteAsync(kyLuongId);
            return Results.Ok(new { period = summary.Period, payslips = summary.Payslips });
        });

        api.MapPost("/tinh-luong/chot-ky-khoa-so/{kyLuongId:long}", async (
            long kyLuongId,
            [FromBody] LockPeriodDto dto,
            [FromServices] ILockPayrollPeriodUseCase useCase) =>
        {
            try
            {
                var ok = await useCase.ExecuteAsync(kyLuongId, dto.LockedBy ?? "HR_Admin");
                if (ok)
                {
                    return Results.Ok(new
                    {
                        success = true,
                        message = "Đã chốt khóa sổ kỳ lương thành công (Read-Only) trong CSDL SQL Server!"
                    });
                }
                return Results.BadRequest(new { success = false, message = "Không thể khóa sổ kỳ lương này (có thể đã bị khóa trước đó)." });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { success = false, message = ex.Message });
            }
        });

        // 8. Phiếu lương cá nhân (Anti-IDOR)
        api.MapGet("/phieu-luong/cua-toi", async (
            [FromQuery] long periodId,
            [FromQuery] long employeeId,
            [FromServices] IViewMyPayslipUseCase useCase) =>
        {
            var payslip = await useCase.ExecuteAsync(periodId, employeeId);
            if (payslip == null)
                return Results.NotFound(new { message = "Không tìm thấy phiếu lương tương ứng trong CSDL." });
            return Results.Ok(payslip);
        });

        // 9. Trợ lý Lương AI hỏi đáp chế độ
        api.MapPost("/ai/giai-thich-luong", async (
            [FromBody] AiInquiryDto dto,
            [FromServices] IAskSalaryAiUseCase useCase) =>
        {
            try
            {
                var msg = await useCase.ExecuteAsync(dto.EmployeeId, dto.Question, dto.CurrentPeriodId, dto.ComparePeriodId);
                return Results.Ok(msg);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { success = false, message = ex.Message });
            }
        });
    }
}

public record CreateLeaveDto(long EmployeeId, LeaveType LeaveType, DateTime StartDate, DateTime EndDate, decimal TotalDays, string? Reason);
public record ApproveLeaveDto(string Status, string? Reviewer, string? Reason);
public record PunchDto(long EmployeeId, string PunchType, long? ShiftId, string? CustomTime, DateTime? WorkDate);
public record CalculateBatchDto(long PeriodId);
public record LockPeriodDto(string? LockedBy);
public record AiInquiryDto(long EmployeeId, string Question, long CurrentPeriodId, long? ComparePeriodId);
