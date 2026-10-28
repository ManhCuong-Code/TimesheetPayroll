using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using TimesheetPayroll.CoreBusiness.Services;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;
using TimesheetPayroll.UseCases.AdminPortal;
using TimesheetPayroll.UseCases.Authentication;
using TimesheetPayroll.UseCases.EmployeePortal;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;
using TimesheetPayroll.UseCases.PluginInterfaces.Services;
using TimesheetPayroll.UseCases.PluginInterfaces.StateStore;
using TimesheetPayroll.DataStore.SQL.Dapper;
using TimesheetPayroll.StateStore.DI;
using TimesheetPayroll.Web;
using TimesheetPayroll.Web.Components;
using TimesheetPayroll.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình UTF-8 toàn vẹn cho JSON Serialization (tránh escaping các ký tự tiếng Việt có dấu)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
});

// 1. ADD BLAZOR INTERACTIVE SERVER & AUTHENTICATION
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(20);
    })
    .AddHubOptions(options =>
    {
        // Tăng ngưỡng khung tin nhắn SignalR (mặc định 32KB) lên 512KB để tránh lỗi TaskCanceledException khi truyền vector 128-D + ảnh thumbnail
        options.MaximumReceiveMessageSize = 512 * 1024;
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
        options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    });

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Bật CORS cho phép kết nối giao tiếp 2 chiều từ mọi trình duyệt và Client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddScoped<IUserSessionService>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());

// Dịch vụ lưu trữ và xác thực ảnh chấm công
builder.Services.AddScoped<IAttendancePhotoService, AttendancePhotoService>();

// 2. PLUGINS (SQL DAPPER & STATE STORE)
builder.Services.AddSingleton<IDataAccess, DataAccess>();
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ITimesheetRepository, TimesheetRepository>();
builder.Services.AddScoped<IPayrollPeriodRepository, PayrollPeriodRepository>();
builder.Services.AddScoped<IPayslipRepository, PayslipRepository>();
builder.Services.AddScoped<IDeductionRateRepository, DeductionRateRepository>();
builder.Services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
builder.Services.AddScoped<IWorkShiftRepository, WorkShiftRepository>();
builder.Services.AddScoped<ITimesheetStateStore, TimesheetStateStore>();
builder.Services.AddScoped<IFaceProfileRepository, FaceProfileRepository>();
builder.Services.AddScoped<IFaceAuditLogRepository, FaceAuditLogRepository>();
builder.Services.AddScoped<ITimesheetAuditLogRepository, TimesheetAuditLogRepository>();
builder.Services.AddScoped<IAttendanceRequestRepository, AttendanceRequestRepository>();
builder.Services.AddScoped<IShiftAssignmentRepository, ShiftAssignmentRepository>();

// 3. CORE SERVICES & AI ASSISTANT
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IFaceBiometricService, FaceBiometricService>();
builder.Services.AddHostedService<BiometricStartupWarmupService>();
builder.Services.AddTransient<IAttendanceShiftResolver, AttendanceShiftResolver>();
builder.Services.AddSingleton<IAiKnowledgeBaseService>(sp =>
{
    var kb = new AiKnowledgeBaseService();
    var customNotes = builder.Configuration["AiSettings:CustomKnowledgeBase"];
    if (!string.IsNullOrWhiteSpace(customNotes))
    {
        kb.SetCustomNotes(customNotes);
    }
    return kb;
});

builder.Services.AddSingleton<ILlmClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient();
    var client = new LlmClient(httpClient);

    var providerStr = builder.Configuration["AiSettings:Provider"];
    var apiKey = builder.Configuration["AiSettings:ApiKey"];
    var model = builder.Configuration["AiSettings:Model"] ?? "gemini-3.8-flash";
    var isEnabled = builder.Configuration.GetValue<bool>("AiSettings:IsEnabled", true);

    var provider = TimesheetPayroll.CoreBusiness.Models.AiProviderType.Gemini;
    if (string.Equals(providerStr, "OpenAI", StringComparison.OrdinalIgnoreCase))
        provider = TimesheetPayroll.CoreBusiness.Models.AiProviderType.OpenAi;
    else if (string.Equals(providerStr, "Local", StringComparison.OrdinalIgnoreCase))
        provider = TimesheetPayroll.CoreBusiness.Models.AiProviderType.LocalEngine;

    string envKey = provider == TimesheetPayroll.CoreBusiness.Models.AiProviderType.OpenAi
        ? (Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty)
        : (Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty);

    client.UpdateConfiguration(new TimesheetPayroll.CoreBusiness.Models.AiConfiguration
    {
        Provider = provider,
        ApiKey = !string.IsNullOrWhiteSpace(apiKey) ? apiKey : envKey,
        Model = model,
        IsEnabled = isEnabled
    });

    return client;
});

builder.Services.AddTransient<ISalaryFormulaEngine, SalaryFormulaEngine>();
builder.Services.AddTransient<ITimesheetEvaluationService, TimesheetEvaluationService>();
builder.Services.AddTransient<IPayrollCalculationService, PayrollCalculationService>();
builder.Services.AddTransient<ISalaryAiAssistantService, SalaryAiAssistantService>();

// 4. AUTHENTICATION USE CASES
builder.Services.AddTransient<ILoginUseCase, LoginUseCase>();
builder.Services.AddTransient<IGetUserAccountsUseCase, GetUserAccountsUseCase>();
builder.Services.AddTransient<IChangePasswordUseCase, ChangePasswordUseCase>();

// 5. ADMIN PORTAL USE CASES
builder.Services.AddTransient<ICalculatePayrollBatchUseCase, CalculatePayrollBatchUseCase>();
builder.Services.AddTransient<IViewPayrollSummaryUseCase, ViewPayrollSummaryUseCase>();
builder.Services.AddTransient<ILockPayrollPeriodUseCase, LockPayrollPeriodUseCase>();
builder.Services.AddTransient<IUnlockPayrollPeriodUseCase, UnlockPayrollPeriodUseCase>();
builder.Services.AddTransient<IViewTimesheetSummaryUseCase, ViewTimesheetSummaryUseCase>();
builder.Services.AddTransient<IProcessLeaveRequestUseCase, ProcessLeaveRequestUseCase>();
builder.Services.AddTransient<IExportBankTransferUseCase, ExportBankTransferUseCase>();
builder.Services.AddTransient<ICreateUserAccountUseCase, CreateUserAccountUseCase>();
builder.Services.AddTransient<IUpdateUserAccountUseCase, UpdateUserAccountUseCase>();
builder.Services.AddTransient<IGetEmployeesWithoutAccountUseCase, GetEmployeesWithoutAccountUseCase>();
builder.Services.AddTransient<IToggleUserStatusUseCase, ToggleUserStatusUseCase>();
builder.Services.AddTransient<IGetWorkShiftsUseCase, GetWorkShiftsUseCase>();
builder.Services.AddTransient<IManageWorkShiftUseCase, ManageWorkShiftUseCase>();
builder.Services.AddTransient<IExportPayrollExcelUseCase, ExportPayrollExcelUseCase>();
builder.Services.AddTransient<IFaceManagementUseCases, FaceManagementUseCases>();
builder.Services.AddTransient<IExportTimesheetExcelUseCase, ExportTimesheetExcelUseCase>();
builder.Services.AddTransient<IManageTimesheetUseCase, ManageTimesheetUseCase>();
builder.Services.AddTransient<IProcessAttendanceRequestUseCase, ProcessAttendanceRequestUseCase>();
builder.Services.AddTransient<IGetShiftAssignmentsUseCase, GetShiftAssignmentsUseCase>();
builder.Services.AddTransient<IManageShiftAssignmentUseCase, ManageShiftAssignmentUseCase>();

// 6. EMPLOYEE PORTAL USE CASES
builder.Services.AddTransient<IViewMyPayslipUseCase, ViewMyPayslipUseCase>();
builder.Services.AddTransient<IViewMyTimesheetUseCase, ViewMyTimesheetUseCase>();
builder.Services.AddTransient<ICreateLeaveRequestUseCase, CreateLeaveRequestUseCase>();
builder.Services.AddTransient<ICheckInCheckOutUseCase, CheckInCheckOutUseCase>();
builder.Services.AddTransient<IFaceCheckInUseCase, FaceCheckInUseCase>();
builder.Services.AddTransient<IGetEmployeeProfileUseCase, GetEmployeeProfileUseCase>();
builder.Services.AddTransient<IGetEmployeePayslipHistoryUseCase, GetEmployeePayslipHistoryUseCase>();
builder.Services.AddTransient<ICompareSalaryUseCase, CompareSalaryUseCase>();
builder.Services.AddTransient<IAskSalaryAiUseCase, AskSalaryAiUseCase>();
builder.Services.AddTransient<ISimulateSalaryUseCase, SimulateSalaryUseCase>();
builder.Services.AddTransient<ICreateAttendanceRequestUseCase, CreateAttendanceRequestUseCase>();

var app = builder.Build();

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStatusCodePagesWithReExecute("/404");
app.UseCors("AllowAll");

// Đảm bảo mọi phản hồi JSON đều mang charset=utf-8 rõ ràng
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        if (context.Response.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true &&
            !context.Response.ContentType.Contains("charset", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.ContentType += "; charset=utf-8";
        }
        return Task.CompletedTask;
    });
    await next();
});

app.UseAntiforgery();
app.UseStaticFiles();

// Đăng ký toàn bộ các Endpoint REST API giao tiếp 2 chiều với CSDL SQL Server (BangChamCongDB)
app.MapTimesheetPayrollApi();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(
        typeof(TimesheetPayroll.Web.EmployeePortal.Pages.MyPayslipComponent).Assembly,
        typeof(TimesheetPayroll.Web.AdminPortal.Pages.PayrollBatchCalculationPage).Assembly
    );

app.Run();
