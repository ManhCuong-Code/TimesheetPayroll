using Microsoft.AspNetCore.Components.Authorization;
using TimesheetPayroll.CoreBusiness.Services;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;
using TimesheetPayroll.UseCases.AdminPortal;
using TimesheetPayroll.UseCases.Authentication;
using TimesheetPayroll.UseCases.EmployeePortal;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;
using TimesheetPayroll.DataStore.SQL.Dapper;
using TimesheetPayroll.StateStore.DI;
using TimesheetPayroll.Web;
using TimesheetPayroll.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// 1. ADD BLAZOR INTERACTIVE SERVER & AUTHENTICATION
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());

// 2. PLUGINS (SQL DAPPER & STATE STORE)
builder.Services.AddSingleton<IDataAccess, DataAccess>();
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();
builder.Services.AddScoped<IPayrollPeriodRepository, PayrollPeriodRepository>();
builder.Services.AddScoped<IPayslipRepository, PayslipRepository>();
builder.Services.AddScoped<IDeductionRateRepository, DeductionRateRepository>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<ITimesheetStateStore, TimesheetStateStore>();

// 3. CORE SERVICES
builder.Services.AddTransient<IPayrollCalculationService, PayrollCalculationService>();

// 4. AUTHENTICATION USE CASES
builder.Services.AddTransient<ILoginUseCase, LoginUseCase>();
builder.Services.AddTransient<IGetUserAccountsUseCase, GetUserAccountsUseCase>();

// 5. ADMIN PORTAL USE CASES
builder.Services.AddTransient<ICalculatePayrollBatchUseCase, CalculatePayrollBatchUseCase>();
builder.Services.AddTransient<IViewPayrollSummaryUseCase, ViewPayrollSummaryUseCase>();
builder.Services.AddTransient<ILockPayrollPeriodUseCase, LockPayrollPeriodUseCase>();
builder.Services.AddTransient<IViewTimesheetSummaryUseCase, ViewTimesheetSummaryUseCase>();
builder.Services.AddTransient<IProcessLeaveRequestUseCase, ProcessLeaveRequestUseCase>();

// 6. EMPLOYEE PORTAL USE CASES
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
