echo Terminating "%1.exe"
taskkill /f /im "%1.exe" 2>nul 1>nul
exit 0