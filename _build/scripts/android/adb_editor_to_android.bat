@echo off
SET ADB_PATH="C:\Program Files (x86)\Android\android-sdk\platform-tools"
SET PATH=%ADB_PATH%;%PATH%
adb shell run-as com.CompanyName.MonkeyPaste.Avalonia "rm -fr files/MonkeyPaste.Editor && exit"
adb push C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Editor\ /sdcard/
adb shell run-as com.CompanyName.MonkeyPaste.Avalonia "mv /sdcard/MonkeyPaste.Editor  files/MonkeyPaste.Editor && exit"
pause