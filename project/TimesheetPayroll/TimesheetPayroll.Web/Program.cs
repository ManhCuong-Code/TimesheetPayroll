using TimesheetPayroll.CoreBusiness.Services;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;
using TimesheetPayroll.UseCases.AdminPortal;
using TimesheetPayroll.UseCases.EmployeePortal;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;
using TimesheetPayroll.DataStore.SQL.Dapper;
using TimesheetPayroll.StateStore.DI;
using TimesheetPayroll.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// 1. ADD BLAZOR INTERACTIVE SERVER
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 2. PLUGINS (SQL DAPPER & STATE STORE)
builder.Services.AddSingleton<IDataAccess, DataAccess>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();
builder.Services.AddScoped<IPayrollPeriodRepository, PayrollPeriodRepository>();
builder.Services.AddScoped<IPayslipRepository, PayslipRepository>();
builder.Services.AddScoped<IDeductionRateRepository, DeductionRateRepository>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ITimesheetStateStore, TimesheetStateStore>();

// 3. CORE SERVICES
builder.Services.AddTransient<IPayrollCalculationService, PayrollCalculationService>();

// 4. ADMIN PORTAL USE CASES
builder.Services.AddTransient<ICalculatePayrollBatchUseCase, CalculatePayrollBatchUseCase>();
builder.Services.AddTransient<IViewPayrollSummaryUseCase, ViewPayrollSummaryUseCase>();
builder.Services.AddTransient<ILockPayrollPeriodUseCase, LockPayrollPeriodUseCase>();
builder.Services.AddTransient<IViewTimesheetSummaryUseCase, ViewTimesheetSummaryUseCase>();
builder.Services.AddTransient<IProcessLeaveRequestUseCase, ProcessLeaveRequestUseCase>();

// 5. EMPLOYEE PORTAL USE CASES
builder.Services.AddTransient<IViewMyPayslipUseCase, ViewMyPayslipUseCase>();
builder.Services.AddTransient<IViewMyTimesheetUseCase, ViewMyTimesheetUseCase>();
builder.Services.AddTransient<ICreateLeaveRequestUseCase, CreateLeaveRequestUseCase>();
builder.Services.AddTransient<ICheckInCheckOutUseCase, CheckInCheckOutUseCase>();

var app = builder.Build();

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
