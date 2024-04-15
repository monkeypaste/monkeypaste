#!/bin/bash
echo "Building..."
TARGET_FRAMEWORK="net8.0"
cd "../../../../MonkeyPaste.Desktop/"
dotnet restore

dotnet build

chmod +x "bin/Debug/$TARGET_FRAMEWORK/linux-x64//MonkeyPaste.Desktop" 

cd -
echo "DONE"