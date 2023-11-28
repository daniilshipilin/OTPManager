@echo off

set "cert=%CodesignCertPath%"
set "timestamp=http://timestamp.digicert.com"

set "src=.\bin\publish"

signtool.exe sign /fd sha256 /a /f "%cert%" "%src%\OTPManager.exe"
signtool.exe timestamp /tr "%timestamp%" /td sha256 "%src%\OTPManager.exe"

signtool.exe sign /fd sha256 /a /f "%cert%" "%src%\OTPManager.dll"
signtool.exe timestamp /tr "%timestamp%" /td sha256 "%src%\OTPManager.dll"

del /S /Q ".\bin\OTPManager.zip" >nul 2>&1
7za.exe a -mx0 -tzip ".\bin\OTPManager.zip" "%src%\*" -xr!*.pdb
7za.exe h -scrcSHA256 ".\bin\OTPManager.zip" > ".\bin\sha256.txt"

pause
