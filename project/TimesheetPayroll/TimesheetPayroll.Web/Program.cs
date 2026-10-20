using Microsoft.AspNetCore.Components.Authorization;
using TimesheetPayroll.CoreBusiness.Services;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;
using TimesheetPayroll.UseCases.AdminPortal;
using TimesheetPayroll.UseCases.Authentication;
using TimesheetPayroll.UseCases.EmployeePortal;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;
using TimesheetPayroll.UseCases.PluginInterfaces.StateStore;
using TimesheetPayroll.DataStore.SQL.Dapper;
using TimesheetPayroll.StateStore.DI;
using TimesheetPayroll.Web;
using TimesheetPayroll.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// 1. ADD BLAZOR INTERACTIVE SERVER & AUTHENTICATION
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddScoped<IUserSessionService>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());

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

// 3. CORE SERVICES & AI ASSISTANT
builder.Services.AddHttpClient();
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
    var model = builder.Configuration["AiSettings:Model"] ?? "gemini-2.0-flash";
    var isEnabled = builder.Configuration.GetValue<bool>("AiSettings:IsEnabled", true);

    var provider = TimesheetPayroll.CoreBusiness.Models.AiProviderType.Gemini;
    if (string.Equals(providerStr, "OpenAI", StringComparison.OrdinalIgnoreCase))
        provider = TimesheetPayroll.CoreBusiness.Models.AiProviderType.OpenAi;
    else if (string.Equals(providerStr, "Local", StringComparison.OrdinalIgnoreCase))
        provider = TimesheetPayroll.CoreBusiness.Models.AiProviderType.LocalEngine;

    client.UpdateConfiguration(new TimesheetPayroll.CoreBusiness.Models.AiConfiguration
    {
        Provider = provider,
        ApiKey = !string.IsNullOrWhiteSpace(apiKey) ? apiKey : (Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty),
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

// 6. EMPLOYEE PORTAL USE CASES
builder.Services.AddTransient<IViewMyPayslipUseCase, ViewMyPayslipUseCase>();
builder.Services.AddTransient<IViewMyTimesheetUseCase, ViewMyTimesheetUseCase>();
builder.Services.AddTransient<ICreateLeaveRequestUseCase, CreateLeaveRequestUseCase>();
builder.Services.AddTransient<ICheckInCheckOutUseCase, CheckInCheckOutUseCase>();
builder.Services.AddTransient<IGetEmployeeProfileUseCase, GetEmployeeProfileUseCase>();
builder.Services.AddTransient<IGetEmployeePayslipHistoryUseCase, GetEmployeePayslipHistoryUseCase>();
builder.Services.AddTransient<ICompareSalaryUseCase, CompareSalaryUseCase>();
builder.Services.AddTransient<IAskSalaryAiUseCase, AskSalaryAiUseCase>();
builder.Services.AddTransient<ISimulateSalaryUseCase, SimulateSalaryUseCase>();

var app = builder.Build();

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/404");
app.UseAntiforgery();
app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(
        typeof(TimesheetPayroll.Web.EmployeePortal.Pages.MyPayslipComponent).Assembly,
        typeof(TimesheetPayroll.Web.AdminPortal.Pages.PayrollBatchCalculationPage).Assembly
    );

app.Run();
