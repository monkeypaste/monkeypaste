#!/bin/bash
CONFIG=Debug
FRAMEWORK="net8.0-ios"
RUNTIME="ios-arm64"
PLATFORM="AnyCPU"
DEVICE_ID="00008020-001945DA3669402E"
DEVICE_ARG="-p:_DeviceName="
EXE_NAME="iosKeyboardTest.iOS.app"

clear

if [ "$1" = "sim" ] || [ "$2" = "sim" ]; then
	RUNTIME="iossimulator-x64"
	DEVICE_ID="D57D4990-6DCF-4D75-AC49-C3B7AD0959F4"
	DEVICE_ARG="-p:_DeviceName=:v2:udid="
fi

#dotnet build -c ${CONFIG}
#cd bin/${CONFIG}/${FRAMEWORK}/${RUNTIME}
#ios-deploy --debug --bundle my.app
#dotnet build -t:Run -f ${FRAMEWORK} -p:Platform=${PLATFORM} -p:RuntimeIdentifier=${RUNTIME} ${DEVICE_ARG}${DEVICE_ID}
dotnet publish -c ${CONFIG} -f ${FRAMEWORK} -p:RuntimeIdentifier=${RUNTIME} ${DEVICE_ARG}${DEVICE_ID}
