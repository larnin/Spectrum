@echo off

echo Installing manager...
if exist "%DISTANCE_PATH%\Distance_Data\Spectrum\Spectrum.Manager.dll" ( 
	echo Removing older version of Spectrum.Manager.dll...
	del "%DISTANCE_PATH%\Distance_Data\Spectrum\Spectrum.Manager.dll"
)
copy "%~dp0\Spectrum.Manager\bin\Debug\Spectrum.Manager.dll" "%DISTANCE_PATH%\Distance_Data\Spectrum\Spectrum.Manager.dll"

echo Installing API...

if exist "%DISTANCE_PATH%\Distance_Data\Spectrum\Spectrum.API.dll" (
	echo Removing older version of Spectrum.API.dll...
	del /S /Q "%DISTANCE_PATH%\Distance_Data\Spectrum\Spectrum.API.dll"
)
copy "%~dp0\Spectrum.API\bin\Debug\Spectrum.API.dll" "%DISTANCE_PATH%\Distance_Data\Spectrum\Spectrum.API.dll"

set ERRORLEVEL=0