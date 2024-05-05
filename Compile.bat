@echo off

:: Update NuGet package cache
echo Updating NuGet package cache...
dotnet nuget locals all --clear

:: Set the port for the application
set PORT=7500

:: Set the ASP.NET Core environment
set ASPNETCORE_ENVIRONMENT=Development

:: Check if .NET 6 SDK is installed
dotnet --list-sdks | findstr "6\." > nul
if errorlevel 1 (
    echo ERROR: .NET 6 SDK is not installed. Please install it and try again.
    exit /b 1
)

:: Change to the current directory
cd /d %~dp0

:: Restore dependencies
echo Restoring dependencies...
dotnet restore

:: Compile with watch
echo Compiling with watch...
start /B cmd /c "set ASPNETCORE_URLS=http://localhost:%PORT% && set ASPNETCORE_ENVIRONMENT=%ASPNETCORE_ENVIRONMENT% && dotnet watch build"

:: Open browser to application root
echo Opening browser to application...
start http://localhost:%PORT%

pause
