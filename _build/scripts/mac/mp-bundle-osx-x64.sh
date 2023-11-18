#!/bin/sh
INFO_PLIST="Info.plist"
ICON_FILE="MyIcon.icns"

PROJECT_DIR="/Users/tkefauver/mp/MonkeyPaste.Avalonia.Desktop"
PROJECT_TARGET_DIR="$PROJECT_DIR/bin/Debug/net8.0/osx-x64"
PUBLISH_DIR="$PROJECT_TARGET_DIR/publish/"

APP_PATH="$PROJECT_TARGET_DIR/MonkeyPaste.app"
APP_TARGET_PATH="$APP_PATH/Contents/MacOS"
DAT_DIR="/Users/tkefauver/mp/Plugins/dat"

cd "$PROJECT_DIR"

dotnet publish -r osx-x64 --configuration Debug -p:UseAppHost=true --self-contained true


if [ -d "$APP_PATH" ]
then
    rm -rf "$APP_PATH"
fi

mkdir "$APP_PATH"

mkdir "$APP_PATH/Contents"
mkdir "$APP_PATH/Contents/MacOS"
mkdir "$APP_PATH/Contents/Resources"

cp "$INFO_PLIST" "$APP_PATH/Contents/Info.plist"
cp "$ICON_FILE" "$APP_PATH/Contents/Resources/$ICON_FILE"
cp -a "$PUBLISH_DIR" "$APP_PATH/Contents/MacOS"

chmod +x "$APP_TARGET_PATH/CefGlueBrowserProcess/Xilium.CefGlue.BrowserProcess"
chmod +x "$APP_TARGET_PATH/MonkeyPaste.Avalonia.Desktop"

cp -r "$DAT_DIR" "$APP_TARGET_PATH"

if [ "$1" = "remote_exec" ]; then 
cd "$APP_TARGET_PATH"
./MonkeyPaste.Avalonia.Desktop --wait-for-attach
fi