@ECHO OFF
SET PY_PLUGIN_PATH=%1
SET REQ=%2
python %PY_PLUGIN_PATH% %REQ%
rem pause
rem FOR /F "delims=" %%i IN ('python %PY_PLUGIN_PATH% %REQ%') DO ECHO %%i