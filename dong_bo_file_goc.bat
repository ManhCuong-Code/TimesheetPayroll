@echo off
echo Dang kiem tra va sao chep ban cap nhat vao Bai_trinh_bay_Lap_trinh_ung_dung_Web.pptx...
copy /Y "Bai_trinh_bay_Lap_trinh_ung_dung_Web_CapNhat.pptx" "Bai_trinh_bay_Lap_trinh_ung_dung_Web.pptx"
if %errorlevel% equ 0 (
    echo [THANH CONG] Da cap nhat truc tiep vao file goc Bai_trinh_bay_Lap_trinh_ung_dung_Web.pptx!
) else (
    echo [LUU Y] PowerPoint van dang mo file. Vui long dong PowerPoint roi chay lai file bat nay.
)
pause
