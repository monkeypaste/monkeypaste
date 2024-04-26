#!/bin/bash
echo "Building..."
CONFIG=Debug
PLATFORM=linux-x64
FRAMEWORK="net8.0"

cd "../../../../MonkeyPaste.Desktop/"

dotnet build -c $CONFIG -f $FRAMEWORK
#dotnet msbuild /property:Configuration=$CONFIG /property:DefineConstants=LINUX%3BDESKTOP%3BCEFNET_WV%3B$CONFIG -restore

chmod +x "bin/$CONFIG/$FRAMEWORK/$PLATFORM/MonkeyPaste.Desktop" 

cd -
echo "DONE"