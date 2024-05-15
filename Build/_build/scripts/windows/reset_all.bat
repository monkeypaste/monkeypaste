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

rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Android\obj\"
rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Android\bin\"

rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.iOS\obj\"
rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.iOS\bin\"

rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Web\obj\"
rmdir /S /Q "C:\Users\tkefauver\Source\Repos\MonkeyPaste\MonkeyPaste.Web\bin\"

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

@echo OFF
echo 0. Shutdown Visual Studio if open
echo 1. Clean out solution file (remove everything inside 'GlobalSection(ProjectConfigurationPlatforms) = postSolution') before reopening.
echo 2. Remove and add MonkeyPaste.Common ref to MonkeyPaste
echo 3. Remove and add MonkeyPaste.Common.Plugin ref to MonkeyPaste.Common

pause

