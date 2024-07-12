#!/bin/bash
CONFIG=Debug
FRAMEWORK="net8.0-ios"
RUNTIME="ios-arm64"
PLATFORM="AnyCPU"
DEVICE_ID="00008020-001945DA3669402E"
DEVICE_ARG="-p:_DeviceName="
EXE_NAME="iosKeyboardTest.iOS"

clear


# NOTE these auto-gen files break build saying TargetRuntime doesn't match PlatformTarget
rm -fr *.csproj.user
rm -fr ../iosKeyboardTest.iOS.KeyboardExt/*.csproj.user

rm -fr obj/
rm -fr bin/

rm -fr ../iosKeyboardTest.iOS.KeyboardExt/bin
rm -fr ../iosKeyboardTest.iOS.KeyboardExt/obj

rm -fr ../iosKeyboardTest.iOS/obj
rm -fr ../iosKeyboardTest.iOS/bin

if [ "$1" = "sim" ] || [ "$2" = "sim" ]; then
	RUNTIME="iossimulator-x64"
	# ipad 17.2
	#DEVICE_ID="D57D4990-6DCF-4D75-AC49-C3B7AD0959F4"
	# ipad 17.4 4th gen
	DEVICE_ID="D533CCC7-612A-479E-A0E6-4898E27D519F"
	# ipad 17.4 M4
	#DEVICE_ID="88D47586-6855-4CF7-805F-EFB0996B7180"
	DEVICE_ARG="-p:_DeviceName=:v2:udid="
fi

if [ "$1" = "manual" ] || [ "$2" = "manual" ]; then
	dotnet publish -c ${CONFIG} -f ${FRAMEWORK} -p:RuntimeIdentifier=${RUNTIME} ${DEVICE_ARG}${DEVICE_ID}
	
	cd bin/${CONFIG}/${FRAMEWORK}/${RUNTIME}/publish

	# unzip the IPA file to tmp folder
	mkdir ./tmp
	unzip ${EXE_NAME}.ipa -d ./tmp
	#cd -

	# run ios-deploy to install the app into iOS device
	ios-deploy -r -b ./tmp/Payload/*.app -O "/Users/tkefauver/Desktop/output.log" -E "/Users/tkefauver/Desktop/error.log"
	rm -r ./tmp
	#CUR_DIR=`echo pwd`
	#./ios-ebee-deploy.sh -b "${CUR_DIR}/bin/${CONFIG}/${FRAMEWORK}/${RUNTIME}/publish/tmp/Payload/${EXE_NAME}.app" -i "${DEVICE_ID}" -l "${CUR_DIR}\output.log"
else
	dotnet build -t:Run -f ${FRAMEWORK} -p:Platform=${PLATFORM} ${DEVICE_ARG}${DEVICE_ID} -p:RuntimeIdentifier=${RUNTIME}
fi