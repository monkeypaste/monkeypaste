#!/bin/sh

dotnet msbuild -t:BundleApp -p:RuntimeIdentifier=osx-x64 -p:Platform=x64

TARGETAPP=bin/x64/Debug/net7.0/osx-x64/publish/MonkeyPaste.app/Contents/MacOS
chmod +x "$TARGETAPP/CefGlueBrowserProcess/Xilium.CefGlue.BrowserProcess"
chmod +x "$TARGETAPP/MonkeyPaste"
