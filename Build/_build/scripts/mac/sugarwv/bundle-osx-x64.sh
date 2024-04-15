#!/bin/sh
TARGET_FRAMEWORK="net8.0"
cd "../../../../../MonkeyPaste.Desktop/"
dotnet restore -r osx-x64

dotnet msbuild -t:BundleApp -p:RuntimeIdentifier=osx-x64 -p:Platform=x64

TARGETAPP="bin/x64/Debug/$TARGET_FRAMEWORK/osx-x64/publish/MonkeyPaste.app/Contents/MacOS"
chmod +x "$TARGETAPP/MonkeyPaste.Desktop" 

cp "MyIcon.icns" "bin/x64/Debug/$TARGET_FRAMEWORK/osx-x64/publish/MonkeyPaste.app/Contents/Resources"

