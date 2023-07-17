@echo off
set SCRIPT_HOME=C:\Users\tkefauver\Source\Repos\MonkeyPaste\_build\scripts\windows
start %SCRIPT_HOME%/build_and_move_plugin.bat AvCoreAnnotator %1
start %SCRIPT_HOME%/build_and_move_plugin.bat AvCoreClipboardHandler %1
exit 0