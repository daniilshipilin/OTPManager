@echo off

set "timestamp=http://timestamp.digicert.com"
set "publishDir=.\bin\publish"

del /S /Q %publishDir% >nul 2>&1
rmdir /S /Q %publishDir% >nul 2>&1
del /S /Q ".\bin\OTPManager.zip" >nul 2>&1

dotnet publish -c Release -v m -o "%publishDir%"

if exist "%publishDir%" (
    signtool.exe sign /fd sha256 /a /f %CodesignCertPath% "%publishDir%\OTPManager.exe"
    signtool.exe timestamp /tr "%timestamp%" /td sha256 "%publishDir%\OTPManager.exe"
    signtool.exe sign /fd sha256 /a /f %CodesignCertPath% "%publishDir%\OTPManager.dll"
    signtool.exe timestamp /tr "%timestamp%" /td sha256 "%publishDir%\OTPManager.dll"
    7za.exe a -mx0 -tzip ".\bin\OTPManager.zip" "%publishDir%\*" -xr!*.pdb
    7za.exe h -scrcSHA256 ".\bin\OTPManager.zip" > ".\bin\sha256.txt"
)

pause
