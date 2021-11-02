@echo off

set "cert=C:\GitSources\CodeSign\Certificates\Illuminati_Software_Inc_Code_Sign.p12"
set "timestamp=http://timestamp.digicert.com"

set "bin=.\bin"
set "src=%bin%\Release\net5.0-windows"

signtool.exe sign /fd sha256 /a /f "%cert%" "%src%\OTPManager.exe"
signtool.exe timestamp /tr "%timestamp%" /td sha256 "%src%\OTPManager.exe"

signtool.exe sign /fd sha256 /a /f "%cert%" "%src%\OTPManager.dll"
signtool.exe timestamp /tr "%timestamp%" /td sha256 "%src%\OTPManager.dll"

signtool.exe sign /fd sha256 /a /f "%cert%" "%src%\ApplicationUpdater.dll"
signtool.exe timestamp /tr "%timestamp%" /td sha256 "%src%\ApplicationUpdater.dll"

del /S /Q "%bin%\OTPManager.zip" >nul 2>&1
7za.exe a -mx0 -tzip "%bin%\OTPManager.zip" "%src%\*"
7za.exe h -scrcSHA256 "%bin%\OTPManager.zip" > "%bin%\sha256.txt"

pause
