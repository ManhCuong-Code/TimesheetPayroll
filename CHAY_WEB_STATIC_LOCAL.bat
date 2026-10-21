@echo off
chcp 65001 >nul
title [TIMESHEETPAYROLL] - KHOI CHAY GIAO DIEN TINH (STATIC LOCALHOST)
color 0E

echo ==========================================================================
echo    🏢 HỆ THỐNG QUẢN LÝ NHÂN SỰ – CHẤM CÔNG – TÍNH LƯƠNG
echo    KHỞI CHẠY GIAO DIỆN TĨNH LOCALHOST (HTML / CSS / JAVASCRIPT)
echo ==========================================================================
echo.

cd /d "%~dp0"

echo Đang mở trình duyệt tại: http://localhost:8000 ...
echo (Để dừng hệ thống, nhấn tổ hợp phím: Ctrl + C)
echo.

start /b "" cmd /c "timeout /t 2 /nobreak >nul && start http://localhost:8000"

python -m http.server 8000

pause
