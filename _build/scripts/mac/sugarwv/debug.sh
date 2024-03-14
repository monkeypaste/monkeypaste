#!/bin/sh
clear
TARGET_FRAMEWORK="net8.0"

if [ "$1" = "reset" ]; then
	../reset_user.sh
	../reset_build.sh
fi

./bundle-osx-x64.sh

cd "/Users/tkefauver/mp/MonkeyPaste.Desktop/bin/x64/Debug/$TARGET_FRAMEWORK/osx-x64/publish/MonkeyPaste.app/Contents/MacOS/"
./MonkeyPaste.Desktop --wait-for-attach
