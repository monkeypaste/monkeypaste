#!/bin/sh
clear

if [ "$1" = "reset" ]; then
	../reset_user.sh
	../reset_build.sh
else
	./move-plugins.sh
fi
./bundle-osx-x64.sh
./bundle-plugins.sh

cd "/Users/tkefauver/mp/MonkeyPaste.Desktop/bin/x64/Debug/net8.0/osx-x64/publish/MonkeyPaste.app/Contents/MacOS/"
./MonkeyPaste.Desktop --wait-for-attach
