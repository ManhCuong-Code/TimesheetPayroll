@echo off
chcp 65001 >nul
title [TIMESHEETPAYROLL] - KHOI CHAY HE THONG WEB BLAZOR .NET 10
color 0B

echo ==========================================================================
echo    🏢 HỆ THỐNG QUẢN LÝ NHÂN SỰ – CHẤM CÔNG – TÍNH LƯƠNG (TIMESHEETPAYROLL)
echo    KHỞI CHẠY MÁY CHỦ BLAZOR INTERACTIVE SERVER + MICROSOFT SQL SERVER
echo ==========================================================================
echo.

cd /d "%~dp0"

echo [1/3] Đang kiểm tra dịch vụ Microsoft SQL Server (SQLEXPRESS)...
powershell -Command "Get-Service -Name '*SQLEXPRESS*' -ErrorAction SilentlyContinue | Where-Object { $_.Status -ne 'Running' } | Start-Service" >nul 2>&1

echo [2/3] Dọn dẹp tiến trình cũ giải phóng cổng mạng (Port 5050, 5000)...
for /f "tokens=5" %%a in ('netstat -aon ^| findstr ":5050" ^| findstr "LISTENING"') do (
    taskkill /f /pid %%a >nul 2>&1
)

echo [3/3] Đang khởi động máy chủ Web .NET 10...
echo       Địa chỉ truy cập: http://localhost:5050 (hoặc http://localhost:5000)
echo       (Để dừng hệ thống, nhấn tổ hợp phím: Ctrl + C)
echo.

start /b "" cmd /c "timeout /t 4 /nobreak >nul && start http://localhost:5050"

dotnet run --project "%~dp0project\TimesheetPayroll\TimesheetPayroll.Web\TimesheetPayroll.Web.csproj" --urls "http://localhost:5050;http://localhost:5000"

pause
