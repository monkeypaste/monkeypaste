echo ON
echo Terminating "%1.exe"
rem pause
rem taskkill /f /im "%1.exe" 2>nul 1>nul
taskkill /f /im "%1.exe" 2>nul 1>nul
exit 0