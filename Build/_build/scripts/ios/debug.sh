#!/bin/bash
CONFIG=Release
FRAMEWORK="net8.0-ios"
RUNTIME="ios-arm64"
PLATFORM="Any CPU"
DEVICE_ID="00008020-001945DA3669402E"

clear

RUN_ARGS="--wait-for-attach"

if [ "$1" = "reset-all" ]; then
	./reset_user.sh
	./reset_build.sh
fi
if [ "$1" = "reset-build" ]; then
	./reset_build.sh
fi
if [ "$1" = "reset-user" ]; then
	./reset_user.sh
fi
if [ "$1" = "no-attach" ] || [ "$2" = "no-attach" ]; then
	RUN_ARGS=""
fi

cd "../../../../MonkeyPaste.Avalonia.iOS/"

# udids found by /Applications/Xcode.app/Contents/Developer/usr/bin/simctl list
# SIM_UDID=3606F702-B6EB-48FC-9A7D-D03A7FF6E6DC


# for physical device:
# -p:_DeviceName=<UDID>
#dotnet build -t:Run -p:_DeviceName=:v2:udid=${SIM_UDID} -p:RuntimeIdentifier=iossimulator-x64 -f ${FRAMEWORK}-ios MonkeyPaste.Avalonia.iOS.csproj
dotnet build -t:Run -f ${FRAMEWORK} -p:RuntimeIdentifier=${RUNTIME} -p:_DeviceName=${DEVICE_ID}