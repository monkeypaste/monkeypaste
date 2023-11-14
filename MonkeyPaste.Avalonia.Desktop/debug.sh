#!/bin/bash

./Users/tkefauver/mp/_build/scripts/bash/reset_all.sh
./mp-bundle-osx-x64.sh
cd /Users/tkefauver/mp/MonkeyPaste.Avalonia.Desktop/bin/Debug/net7.0/osx-x64/MonkeyPaste.app/Contents/MacOS/
chmod +x MonkeyPaste.Avalonia.Desktop
./MonkeyPaste.Avalonia.Desktop --wait-for-attach