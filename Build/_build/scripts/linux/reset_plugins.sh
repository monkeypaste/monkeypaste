#!/bin/sh

echo "Resetting Plugins..."
cd "../../../../MonkeyPaste.Avalonia/Assets/dat"

if [ -f $HOME/.local/share/MonkeyPaste_DEBUG ]; then
	rm -fr "$HOME/.local/share/MonkeyPaste_DEBUG/Plugins/cf2ec03f-9edd-45e9-a605-2a2df71e03bd"
	unzip -q cf2ec03f-9edd-45e9-a605-2a2df71e03bd.zip -d ~/.local/share/MonkeyPaste_DEBUG/Plugins/cf2ec03f-9edd-45e9-a605-2a2df71e03bd

	rm -fr "$HOME/.local/share/MonkeyPaste_DEBUG/Plugins/ecde8e7c-30cf-47ef-a6a9-8f7f439b0a31"
	unzip -q ecde8e7c-30cf-47ef-a6a9-8f7f439b0a31.zip -d ~/.local/share/MonkeyPaste_DEBUG/Plugins/ecde8e7c-30cf-47ef-a6a9-8f7f439b0a31
fi

if [ -f $HOME/.local/share/MonkeyPaste ]; then
	rm -fr "$HOME/.local/share/MonkeyPaste/Plugins/cf2ec03f-9edd-45e9-a605-2a2df71e03bd"
	unzip -q cf2ec03f-9edd-45e9-a605-2a2df71e03bd.zip -d ~/.local/share/MonkeyPaste/Plugins/cf2ec03f-9edd-45e9-a605-2a2df71e03bd

	rm -fr "$HOME/.local/share/MonkeyPaste/Plugins/ecde8e7c-30cf-47ef-a6a9-8f7f439b0a31"
	unzip -q ecde8e7c-30cf-47ef-a6a9-8f7f439b0a31.zip -d ~/.local/share/MonkeyPaste/Plugins/ecde8e7c-30cf-47ef-a6a9-8f7f439b0a31
fi

cd -
echo "DONE"