@echo off

Spectrum.Prism.exe Assembly-CSharp.dll Spectrum.Bootstrap.dll

echo %cmdcmdline% | find /i "%~0" >nul
if not errorlevel 1 pause