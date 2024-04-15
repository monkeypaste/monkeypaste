#!/bin/sh
CONFIGURATION="_DEBUG"
echo "User Reset..."
#rm -fr "$HOME/.local/share/MonkeyPaste$CONFIGURATION"
rm -fr "$HOME/Library/Application Support/MonkeyPaste$CONFIGURATION"
echo "DONE"
