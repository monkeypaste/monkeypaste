rem @echo off
set back="%cd%"

set PLUGIN_NAME="%1"
set BUILD_TARGET_DIR="%2"
set DAT_DIR="%3"
set PLUGIN_GUID="%4"

cd %BUILD_TARGET_DIR%
cd ..

set TEMP_DIR="%cd%\%1"
mkdir %TEMP_DIR%
xcopy /s /e %BUILD_TARGET_DIR% %TEMP_DIR%

7z a %PLUGIN_GUID%.zip %PLUGIN_NAME%

if not exist %DAT_DIR% mkdir %DAT_DIR%
move /y %PLUGIN_GUID%.zip %DAT_DIR%

rd /s /q %TEMP_DIR%


cd %back%
rem exit 0