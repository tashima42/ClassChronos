@echo off

:: Update NuGet package cache
echo Updating NuGet package cache...
dotnet nuget locals all --clear

:: Set the port for the application
set PORT=7177

:: Set the ASP.NET Core environment
set ASPNETCORE_ENVIRONMENT=Development

:: Check if .NET 8 SDK is installed
dotnet --list-sdks | findstr "8\." > nul
if errorlevel 1 (
    echo ERROR: .NET 8 SDK is not installed. Please install it and try again.
    exit /b 1
)

:: Change to the current directory
cd /d %~dp0

:: Restore dependencies
echo Restoring dependencies...
dotnet restore

:: Compile with watch
echo Compiling with watch...
start /B cmd /c "set ASPNETCORE_URLS=https://localhost:%PORT% && set ASPNETCORE_ENVIRONMENT=%ASPNETCORE_ENVIRONMENT% && dotnet watch run"

pause
