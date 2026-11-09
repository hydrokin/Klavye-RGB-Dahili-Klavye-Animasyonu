@echo off
rem Klavye RGB - derleme betigi (.NET Framework 4 derleyicisi her Windows'ta hazir gelir)
setlocal
set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" (
  echo 64 bit csc.exe bulunamadi. .NET Framework 4.x kurulu olmali.
  exit /b 1
)
"%CSC%" /nologo /target:winexe /platform:x64 /codepage:65001 /optimize+ /out:"%~dp0KlavyeRGB.exe" /win32manifest:"%~dp0src\app.manifest" /win32icon:"%~dp0src\app.ico" /resource:"%~dp0src\app.ico",KlavyeRGB.app.ico /r:System.Windows.Forms.dll /r:System.Drawing.dll "%~dp0src\*.cs"
if errorlevel 1 (
  echo.
  echo Derleme BASARISIZ.
  exit /b 1
)
echo Derleme tamam: %~dp0KlavyeRGB.exe
