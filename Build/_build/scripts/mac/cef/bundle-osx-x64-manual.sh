#!/bin/sh
TARGET_FRAMEWORK="net8.0"
INFO_PLIST="Info.plist"
ICON_FILE="MyIcon.icns"

PROJECT_DIR="/Users/tkefauver/mp/MonkeyPaste.Desktop"
PROJECT_TARGET_DIR="$PROJECT_DIR/bin/Debug/$TARGET_FRAMEWORK/osx-x64"
PUBLISH_DIR="$PROJECT_TARGET_DIR/publish/"

APP_PATH="$PROJECT_TARGET_DIR/MonkeyPaste.app"
APP_TARGET_PATH="$APP_PATH/Contents/MacOS"

cd "$PROJECT_DIR"

dotnet restore
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
chmod +x "$APP_TARGET_PATH/MonkeyPaste.Desktop"
