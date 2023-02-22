@echo off
SET ADB_PATH="C:\Program Files (x86)\Android\android-sdk\platform-tools"
SET PATH=%ADB_PATH%;%PATH%
adb shell run-as com.CompanyName.MonkeyPaste.Avalonia "cp files/mp_android.mpcdb /sdcard/ && exit"
adb shell run-as com.CompanyName.MonkeyPaste.Avalonia "cp files/mp_android.mpcdb-shm /sdcard/ && exit"
adb shell run-as com.CompanyName.MonkeyPaste.Avalonia "cp files/mp_android.mpcdb-wal /sdcard/ && exit"
adb pull /sdcard/mp_android.mpcdb C:\Users\tkefauver\Desktop\android_sandbox\
adb pull /sdcard/mp_android.mpcdb-shm C:\Users\tkefauver\Desktop\android_sandbox\
adb pull /sdcard/mp_android.mpcdb-wal C:\Users\tkefauver\Desktop\android_sandbox\
exit 0
