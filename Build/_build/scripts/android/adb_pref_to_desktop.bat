@echo off
SET ADB_PATH="C:\Program Files (x86)\Android\android-sdk\platform-tools"
SET PATH=%ADB_PATH%;%PATH%

adb shell run-as com.Monkey.MonkeyPaste "cp files/pref_android.json /sdcard/ && exit"
adb pull /sdcard/pref_android.json C:\Users\tkefauver\Desktop\android_sandbox\
exit 0
