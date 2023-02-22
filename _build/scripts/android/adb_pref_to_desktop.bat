@echo off
SET ADB_PATH="C:\Program Files (x86)\Android\android-sdk\platform-tools"
SET PATH=%ADB_PATH%;%PATH%
adb shell "run-as com.CompanyName.MonkeyPaste.Avalonia cat /data/user/0/com.CompanyName.MonkeyPaste.Avalonia/files/pref_android.json" > C:\Users\tkefauver\Desktop\android_sandbox\pref_android.json
exit 0
