@echo off

del /S /Q .\bin\Release\net5.0-windows >nul 2>&1
rmdir /S /Q .\bin\Release\net5.0-windows >nul 2>&1

dotnet build -o .\bin\Release\net5.0-windows -c Release
pause
