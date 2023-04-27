REM returns 1 if python on path 0 if not found
@echo off 
WHERE python >nul 2>nul
IF %ERRORLEVEL% EQU 0 (ECHO 1) ELSE (ECHO 0)
EXIT