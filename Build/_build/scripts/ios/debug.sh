#!/bin/bash
CONFIG=Debug
FRAMEWORK="net8.0-ios"
RUNTIME="ios-arm64"
PLATFORM="AnyCPU"
DEVICE_ID="00008020-001945DA3669402E"
DEVICE_ARG="-p:_DeviceName="

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

if [ "$1" = "sim" ] || [ "$2" = "sim" ]; then
	RUNTIME="iossimulator-x64"
	DEVICE_ID="D57D4990-6DCF-4D75-AC49-C3B7AD0959F4"
	DEVICE_ARG="-p:_DeviceName=:v2:udid="
fi

cd "../../../../MonkeyPaste.Avalonia.iOS/"


#dotnet build -t:Run  -f ${FRAMEWORK} -p:RuntimeIdentifier=${RUNTIME} ${DEVICE_ARG}${DEVICE_ID}
dotnet publish -c ${CONFIG} -f ${FRAMEWORK} -p:RuntimeIdentifier=${RUNTIME} ${DEVICE_ARG}${DEVICE_ID}