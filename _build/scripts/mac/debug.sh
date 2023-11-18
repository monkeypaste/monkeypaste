#!/bin/sh
clear

if [ "$1" = "reset"]; then
	./reset_user.sh
fi
./reset_build.sh
./mp-bundle-osx-x64.sh remote_exec
