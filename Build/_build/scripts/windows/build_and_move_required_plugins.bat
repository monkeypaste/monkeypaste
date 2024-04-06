@echo off
set SCRIPT_HOME=C:\Users\tkefauver\Source\Repos\MonkeyPaste\Build\_build\scripts\windows
start %SCRIPT_HOME%/build_and_move_plugin.bat CoreAnnotator %1
start %SCRIPT_HOME%/build_and_move_plugin.bat CoreOleHandler %1
exit 0