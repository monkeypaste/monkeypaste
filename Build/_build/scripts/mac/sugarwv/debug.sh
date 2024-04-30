#!/bin/bash
clear
CONFIG=Release
FRAMEWORK="net8.0"
RUN_ARGS="--wait-for-attach"

if [ "$1" = "reset-all" ]; then
	cd ..
	./reset_user.sh
	./reset_build.sh
	cd -
fi
if [ "$1" = "reset-build" ]; then
	../reset_build.sh
fi
if [ "$1" = "reset-user" ]; then
	../reset_user.sh
fi
if [ "$1" = "no-attach" ]; then
	RUN_ARGS=""
fi

./build.sh

cd "../../../../../MonkeyPaste.Desktop/bin/x64/$CONFIG/$FRAMEWORK/osx-x64/publish/MonkeyPaste.app/Contents/MacOS/"
./MonkeyPaste.Desktop $RUN_ARGS