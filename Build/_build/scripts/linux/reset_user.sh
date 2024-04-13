#!/bin/sh
CONFIGURATION="_DEBUG"
echo "User Reset..."

rm -fr "$HOME/.local/share/MonkeyPaste$CONFIGURATION"

echo "DONE"