#!/bin/bash
echo "Building..."
CONFIG=Debug
PLATFORM=linux-x64
TARGET_FRAMEWORK="net8.0"

cd "../../../../MonkeyPaste.Desktop/"

#dotnet restore -c $CONFIG
#dotnet build -c $CONFIG -f $TARGET_FRAMEWORK
dotnet restore
dotnet build
#dotnet msbuild /property:Configuration=$CONFIG /property:DefineConstants=LINUX%3BDESKTOP%3BCEFNET_WV%3B$CONFIG -restore

chmod +x "bin/$CONFIG/$TARGET_FRAMEWORK/$PLATFORM/MonkeyPaste.Desktop" 

cd -
echo "DONE"