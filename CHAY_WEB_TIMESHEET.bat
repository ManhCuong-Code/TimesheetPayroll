@echo off
chcp 65001 >nul
title [TIMESHEETPAYROLL] - KHOI CHAY HE THONG WEB BLAZOR .NET 10
color 0B

echo ==========================================================================
echo    🏢 HỆ THỐNG QUẢN LÝ NHÂN SỰ – CHẤM CÔNG – TÍNH LƯƠNG (TIMESHEETPAYROLL)
echo    KHỞI CHẠY MÁY CHỦ BLAZOR INTERACTIVE SERVER + MICROSOFT SQL SERVER
echo ==========================================================================
echo.
echo [1/2] Đang mở trình duyệt tại: http://localhost:5050 ...
echo       (Để dừng hệ thống, nhấn tổ hợp phím: Ctrl + C)
echo.

start "" "http://localhost:5050"

dotnet run --project "%~dp0project\TimesheetPayroll\TimesheetPayroll.Web\TimesheetPayroll.Web.csproj" --urls "http://localhost:5050"

pause
