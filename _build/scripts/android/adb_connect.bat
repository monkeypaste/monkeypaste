@echo off
SET ADB_PATH="C:\Program Files (x86)\Android\android-sdk\platform-tools"
SET PATH=%ADB_PATH%;%PATH%
adb kill-server
adb start-server
adb connect 10.68.187.10:5555
pause
