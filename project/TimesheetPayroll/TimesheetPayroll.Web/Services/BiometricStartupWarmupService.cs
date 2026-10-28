using System.Diagnostics;
using TimesheetPayroll.CoreBusiness.Services;
using TimesheetPayroll.CoreBusiness.Services.Interfaces;
using TimesheetPayroll.UseCases.PluginInterfaces.DataStore;

namespace TimesheetPayroll.Web.Services
{
    /// <summary>
    /// Dịch vụ khởi động ngầm (Hosted Service) giúp tự động khôi phục và nạp sẵn (Warm-up)
    /// toàn bộ mẫu khuôn mặt 128-D đang hoạt động từ SQL Server (BangChamCongDB) vào bộ nhớ đệm
    /// của IFaceBiometricService ngay khi khởi động hoặc khởi động lại Server.
    /// Đảm bảo dữ liệu sinh trắc học đã lưu luôn sẵn sàng tức thì sau khi restart mà không cần đăng ký lại.
    /// </summary>
    public class BiometricStartupWarmupService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BiometricStartupWarmupService> _logger;

        public BiometricStartupWarmupService(
            IServiceProvider serviceProvider,
            ILogger<BiometricStartupWarmupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var faceProfileRepo = scope.ServiceProvider.GetRequiredService<IFaceProfileRepository>();
                var biometricService = scope.ServiceProvider.GetRequiredService<IFaceBiometricService>();

                var activeProfiles = await faceProfileRepo.GetAllActiveProfilesAsync();
                int preloadedCount = biometricService.PreloadProfiles(activeProfiles);

                sw.Stop();
                _logger.LogInformation(
                    "[BiometricWarmup] Đã khôi phục và nạp sẵn {Count} mẫu khuôn mặt 128-D từ SQL Server vào bộ nhớ đệm trong {ElapsedMs:0.##} ms.",
                    preloadedCount,
                    sw.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                // Không làm sập tiến trình khởi động web nếu SQL Server đang khởi tạo chậm
                _logger.LogWarning(
                    ex,
                    "[BiometricWarmup] Không thể nạp sẵn mẫu khuôn mặt lúc khởi động ({ElapsedMs:0.##} ms). Hệ thống sẽ tự động nạp khi có truy vấn đầu tiên.",
                    sw.Elapsed.TotalMilliseconds);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
