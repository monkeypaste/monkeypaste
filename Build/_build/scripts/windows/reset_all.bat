@echo OFF
taskkill /f /im "MonkeyPaste.Desktop.exe"

rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\Plugins\CoreOleHandler\obj\"
rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\Plugins\CoreOleHandler\bin\"

rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\Plugins\CoreAnnotator\obj\"
rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\Plugins\CoreAnnotator\bin\"

rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\Common\MonkeyPaste.Common\obj\"
rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\Common\MonkeyPaste.Common\bin\"

rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\Common\MonkeyPaste.Common.Avalonia\obj\"
rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\Common\MonkeyPaste.Common.Avalonia\bin\"

rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\Common\MonkeyPaste.Common.Plugin\obj\"
rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\Common\MonkeyPaste.Common.Plugin\bin\"

rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\Common\MonkeyPaste.Common.Wpf\obj\"
rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\Common\MonkeyPaste.Common.Wpf\bin\"

rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Desktop\obj\"
rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Desktop\bin\"

rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Desktop.Launcher\obj\"
rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Desktop.Launcher\bin\"

rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Avalonia\obj\"
rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Avalonia\bin\"

rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\obj\"
rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste\bin\"

rmdir /S /Q "C:\Users\tkefauver\AppData\Local\MonkeyPaste" 
rmdir /S /Q "C:\Users\tkefauver\AppData\Local\MonkeyPaste_DEBUG" 
rmdir /S /Q "C:\Users\tkefauver\AppData\Roaming\MonkeyPaste" 
rmdir /S /Q "C:\Users\tkefauver\AppData\Roaming\MonkeyPaste_DEBUG" 

echo 1. Clean out solution file
echo 2. Remove and add MonkeyPaste.Common ref to MonkeyPaste
echo 3. Remove and add MonkeyPaste.Common.Plugin ref to MonkeyPaste.Common

pause
