@echo off
SET ADB_PATH="C:\Program Files (x86)\Android\android-sdk\platform-tools"
SET PATH=%ADB_PATH%;%PATH%
adb shell input keyevent KEYCODE_POWER
adb shell input keyevent KEYCODE_POWER
adb shell input touchscreen swipe 580 2150 580 1400
exit 0
