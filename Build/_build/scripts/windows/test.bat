@echo off
set back=%cd%
for /d %%i in ("C:\Users\tkefauver\Source\Repos\MonkeyPaste\Plugins\*") do (
cd "%%i"
IF %%~nxi==unused OR %%~nxi==AvCodeClassifier (echo "skip") ELSE (dotnet clean)
)
cd %back%
for /d %%i in ("C:\Users\tkefauver\Source\Repos\MonkeyPaste\Plugins\*") do (
cd "%%i"
IF %%~nxi==unused OR %%~nxi==AvCodeClassifier  (echo "skip") ELSE (dotnet build)
)