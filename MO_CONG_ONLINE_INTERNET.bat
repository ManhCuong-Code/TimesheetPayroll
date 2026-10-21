@echo off
chcp 65001 >nul
title [TIMESHEETPAYROLL] - MO CONG SERVER RA INTERNET (PUBLIC HTTPS)
color 0A

echo =========================================================================
echo    🏢 HỆ THỐNG QUẢN LÝ NHÂN SỰ – CHẤM CÔNG – TÍNH LƯƠNG
echo    PHÁT MÁY CHỦ BLAZOR .NET 10 + SQL SERVER RA INTERNET CÔNG KHAI (HTTPS)
echo =========================================================================
echo.

cd /d "%~dp0"

echo [1/2] Đang kiểm tra máy chủ Localhost:5050...
curl.exe -s http://localhost:5050/ >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Máy chủ 5050 chưa chạy. Đang tự động khởi động máy chủ .NET 10...
    start "TimesheetPayroll Server" cmd /k "dotnet run --project project\TimesheetPayroll\TimesheetPayroll.Web\TimesheetPayroll.Web.csproj --urls http://localhost:5050"
    timeout /t 5 /nobreak >nul
) else (
    echo Máy chủ Localhost:5050 đang hoạt động tốt!
)

echo.
echo [2/2] Đang kết nối đường hầm bảo mật HTTPS miễn phí ra toàn cầu...
echo.
echo -------------------------------------------------------------------------
echo HƯỚNG DẪN DÀNH CHO BẠN:
echo - Màn hình bên dưới sẽ tự động hiển thị một đường dẫn HTTPS công khai.
echo - Bạn có thể copy đường dẫn đó để mở trên điện thoại, máy tính bảng hoặc
echo   gửi cho thầy cô, hội đồng giám khảo để kiểm thử hệ thống trực tiếp từ xa!
echo -------------------------------------------------------------------------
echo.

ssh -o StrictHostKeyChecking=no -R 80:localhost:5050 nokey@localhost.run

pause
