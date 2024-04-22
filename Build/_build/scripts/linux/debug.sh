#!/bin/bash
clear
CONFIG=Debug
TARGET_FRAMEWORK="net8.0"
PLATFORM=linux-x64 
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
if [ "$1" = "no-attach" ]; then
	RUN_ARGS=" "
fi
if [ "$2" = "no-attach" ]; then
	RUN_ARGS=" "
fi

./build.sh

cd "../../../../MonkeyPaste.Desktop/bin/$CONFIG/$TARGET_FRAMEWORK/$PLATFORM/"
./MonkeyPaste.Desktop $RUN_ARGS