@echo off
cd /d %~dp0

:menu
echo Choose the Docker type:
echo 1. Linux Docker
echo 2. Windows Docker
echo 3. Exit

set /p choice="Enter your choice: "
if "%choice%"=="1" (
    set docker_type=linux
) else if "%choice%"=="2" (
    set docker_type=windows
) else if "%choice%"=="3" (
    exit
) else (
    echo Invalid choice. Please try again.
    goto menu
)

echo Building .NET application...
dotnet publish -c Release

echo Building %docker_type% Docker image...
docker build -t UTFClassAPI:%docker_type% --build-arg DOCKER_TYPE=%docker_type% .

echo Done.
pause
