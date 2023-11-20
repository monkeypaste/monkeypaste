#!/bin/sh
cd "/Users/tkefauver/mp/MonkeyPaste.Avalonia.Desktop/"
dotnet restore -r osx-x64

dotnet msbuild -t:BundleApp -p:RuntimeIdentifier=osx-x64 -p:Platform=x64

TARGETAPP=bin/x64/Debug/net8.0/osx-x64/publish/MonkeyPaste.app/Contents/MacOS
chmod +x "$TARGETAPP/CefGlueBrowserProcess/Xilium.CefGlue.BrowserProcess"
chmod +x "$TARGETAPP/MonkeyPaste.Avalonia.Desktop" 

cp "MyIcon.icns" bin/x64/Debug/net8.0/osx-x64/publish/MonkeyPaste.app/Contents/Resources

