#!/bin/sh
CONFIGURATION="_DEBUG"
echo "User Reset..."

rm -fr "$HOME/.local/share/MonkeyPaste"
rm -fr "$HOME/.local/share/MonkeyPaste_DEBUG"

rm -fr "$HOME/.config/autostart/monkeypaste.desktop"

echo "DONE"