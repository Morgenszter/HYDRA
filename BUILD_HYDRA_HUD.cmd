@echo off
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\BUILD_HYDRA_HUD.ps1"
echo.
pause
