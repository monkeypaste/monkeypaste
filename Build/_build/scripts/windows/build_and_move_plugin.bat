@echo off
set back=%cd%
set SOURCE_PATH="C:\Users\tkefauver\Source\Repos\MonkeyPaste\Plugins\%1"
set TARGET_PATH="%2\%1"
echo Source Path: %SOURCE_PATH%
echo Target Path: %TARGET_PATH%
cd %SOURCE_PATH%
dotnet clean
dotnet build
xcopy /e /k /h /i /y /F "%SOURCE_PATH%" "%TARGET_PATH%"
cd %back% 