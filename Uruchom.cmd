@echo off
cd /d "%~dp0"
set DOTNET_CLI_HOME=%~dp0.local-dotnet
set DOTNET_CLI_TELEMETRY_OPTOUT=1
echo BeFit: http://localhost:5080
echo Po uruchomieniu otworz powyzszy adres. Pozostaw to okno otwarte.
dotnet run --no-launch-profile --urls http://localhost:5080
pause
