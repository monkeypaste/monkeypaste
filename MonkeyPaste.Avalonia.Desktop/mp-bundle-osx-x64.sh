#!/bin/bash

dotnet publish -r osx-x64 --configuration Debug -p:UseAppHost=true --self-contained true

APP_NAME="/Users/tkefauver/mp/MonkeyPaste.Avalonia.Desktop/bin/Debug/net7.0/osx-x64/MonkeyPaste.app"
PUBLISH_OUTPUT_DIRECTORY="/Users/tkefauver/mp/MonkeyPaste.Avalonia.Desktop/bin/Debug/net7.0/osx-x64/publish/"
INFO_PLIST="Info.plist"
ICON_FILE="MyIcon.icns"

if [ -d "$APP_NAME" ]
then
    rm -rf "$APP_NAME"
fi

mkdir "$APP_NAME"

mkdir "$APP_NAME/Contents"
mkdir "$APP_NAME/Contents/MacOS"
mkdir "$APP_NAME/Contents/Resources"

cp "$INFO_PLIST" "$APP_NAME/Contents/Info.plist"
cp "$ICON_FILE" "$APP_NAME/Contents/Resources/$ICON_FILE"
cp -a "$PUBLISH_OUTPUT_DIRECTORY" "$APP_NAME/Contents/MacOS"


TARGETAPP="$APP_NAME/Contents/MacOS"
chmod +x "$TARGETAPP/CefGlueBrowserProcess/Xilium.CefGlue.BrowserProcess"
chmod +x "$TARGETAPP/MonkeyPaste.Avalonia.Desktop"