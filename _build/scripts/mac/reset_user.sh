#!/bin/sh
CONFIGURATION="_DEBUG"
echo "User Reset..."
#rm -fr "/Users/tkefauver/.local/share/MonkeyPaste$CONFIGURATION"
rm -fr "/Users/tkefauver/Library/Application Support/MonkeyPaste$CONFIGURATION"
echo "DONE"
