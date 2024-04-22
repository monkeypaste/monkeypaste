#!/bin/sh
CONFIGURATION="_DEBUG"
echo "User Reset..."

rm -fr "$HOME/.local/share/MonkeyPaste"
rm -fr "$HOME/.local/share/MonkeyPaste_DEBUG"

echo "DONE"