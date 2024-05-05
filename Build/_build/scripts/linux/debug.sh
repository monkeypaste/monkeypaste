#!/bin/bash
clear
CONFIG=Release
FRAMEWORK="net8.0"
PLATFORM=linux-x64 

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

RUN_ARGS="--wait-for-attach"
if [ "$1" = "no-attach" ] || [ "$2" = "no-attach" ]; then
	RUN_ARGS=""
fi

if [ "$1" = "break-on-attach" ] || [ "$2" = "break-on-attach" ]; then
	RUN_ARGS="${RUN_ARGS} --break-on-attach"
fi

./build.sh

RUN_ARGS="${RUN_ARGS} --trace"
cd "../../../../MonkeyPaste.Desktop/bin/${CONFIG}/${FRAMEWORK}/${PLATFORM}/"
./MonkeyPaste.Desktop $RUN_ARGS