@echo off

Spectrum.Prism.exe -t Assembly-CSharp.dll -s Spectrum.Bootstrap.dll

echo %cmdcmdline% | find /i "%~0" >nul
if not errorlevel 1 pause