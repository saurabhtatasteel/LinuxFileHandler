@echo off

set SOURCE=%~1
set DESTINATION=%~2

echo Source: %SOURCE%
echo Destination: %DESTINATION%

copy /Y "%SOURCE%" "%DESTINATION%" >nul


if errorlevel 1 (
    echo 1
) else (
    echo 0
)