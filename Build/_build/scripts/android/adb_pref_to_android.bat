@echo off
SET ADB_PATH="C:\Program Files (x86)\Android\android-sdk\platform-tools"
SET PATH=%ADB_PATH%;%PATH%

adb push C:\Users\tkefauver\Desktop\android_sandbox\pref_android.json /sdcard/ 
adb shell run-as com.Monkey.MonkeyPaste.Avalonia "mv /sdcard/pref_android.json files/pref_android.json && exit"
exit 0
