@echo off
SET ADB_PATH="C:\Program Files (x86)\Android\android-sdk\platform-tools"
SET PATH=%ADB_PATH%;%PATH%
rem adb shell run-as com.Monkey.MonkeyPaste "rm -fr files/MonkeyPaste.Editor && exit"
rem adb push C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Editor\ /sdcard/
adb push --sync C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Avalonia.Web\AppBundle\Editor /sdcard/
rem adb shell run-as com.Monkey.MonkeyPaste "mv /sdcard/MonkeyPaste.Editor  files/MonkeyPaste.Editor && exit"
adb shell run-as com.Monkey.MonkeyPaste "cp -sR /sdcard/Editor  files/Editor && exit"
pause
