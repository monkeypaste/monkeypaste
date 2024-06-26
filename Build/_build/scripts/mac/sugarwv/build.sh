#!/bin/sh
CONFIG=Release
FRAMEWORK="net8.0"
cd "../../../../../MonkeyPaste.Desktop/"
#dotnet restore -r osx-x64

dotnet msbuild /property:Configuration=$CONFIG  -t:BundleApp -p:RuntimeIdentifier=osx-x64 -p:Platform=x64 -restore

TARGETAPP="bin/x64/$CONFIG/$FRAMEWORK/osx-x64/publish/MonkeyPaste.app/Contents/MacOS"
chmod +x "$TARGETAPP/MonkeyPaste.Desktop" 

cp "MyIcon.icns" "bin/x64/$CONFIG/$FRAMEWORK/osx-x64/publish/MonkeyPaste.app/Contents/Resources"

