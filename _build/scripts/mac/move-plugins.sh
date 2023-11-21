#!/bin/sh
echo "Moving plugins..."

TARGET_PLUGIN_DIR="/Users/tkefauver/Library/Application Support/MonkeyPaste_DEBUG/Plugins"

TARGET_CORE_OLE_DIR="$TARGET_PLUGIN_DIR/CoreOleHandler"
rm -fr "$TARGET_CORE_OLE_DIR"

cd "/Users/tkefauver/mp/Plugins/CoreOleHandler"

rm -fr bin
rm -fr obj

dotnet build -r osx-x64

mkdir "$TARGET_CORE_OLE_DIR"
cp -a "bin/Debug/net8.0/osx-x64/." "$TARGET_CORE_OLE_DIR"

echo "CoreOleHandler moved to $TARGET_CORE_OLE_DIR"

TARGET_CORE_ANN_DIR="$TARGET_PLUGIN_DIR/CoreAnnotator"
rm -fr "$TARGET_CORE_ANN_DIR"
cd "/Users/tkefauver/mp/Plugins/CoreAnnotator"

rm -fr bin
rm -fr obj

dotnet build -r osx-x64

mkdir "$TARGET_CORE_ANN_DIR"
cp -a "bin/Debug/net8.0/osx-x64/." "$TARGET_CORE_ANN_DIR"

echo "CoreAnnotator moved to $TARGET_CORE_ANN_DIR"

echo "DONE"
