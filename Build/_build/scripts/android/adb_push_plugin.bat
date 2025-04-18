@echo off
SET ADB_PATH="C:\Program Files (x86)\Android\android-sdk\platform-tools"
SET PATH=%ADB_PATH%;%PATH%
rem SET PACKAGE_NAME=%1
SET PACKAGE_NAME=com.Monkey.MonkeyPaste

rem SET PLUGIN_NAME=%2
SET PLUGIN_NAME=OpenAi

rem SET STORAGE_DIR=%1
SET STORAGE_DIR=/data/user/0/com.Monkey.MonkeyPaste/files
SET TARGET_PLUGIN_ROOT_DIR=%STORAGE_DIR%/Plugins/Declaritive
SET TARGET_PLUGIN_FULL_ROOT_DIR=%STORAGE_DIR%/Plugins/Declaritive

SET TARGET_PLUGIN_DIR=%TARGET_PLUGIN_ROOT_DIR%/%PLUGIN_NAME%/
SET TARGET_PLUGIN_FULL_DIR=%TARGET_PLUGIN_FULL_ROOT_DIR%/%PLUGIN_NAME%/


rem SET SOURCE_PLUGIN_ROOT_DIR=%3
SET SOURCE_PLUGIN_ROOT_DIR=C:\Users\tkefauver\Source\Repos\MonkeyPaste\Plugins\Declarative
SET SOURCE_PLUGIN_DIR=%SOURCE_PLUGIN_ROOT_DIR%\%PLUGIN_NAME%


adb shell "run-as %PACKAGE_NAME% 'mkdir -p %TARGET_PLUGIN_DIR%'"
adb push %SOURCE_PLUGIN_DIR%/. %TARGET_PLUGIN_FULL_DIR%
pause
