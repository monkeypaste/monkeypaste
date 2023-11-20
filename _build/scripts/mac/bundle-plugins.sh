#!/bin/sh
echo "Bundling plugins..."

#DAT_DIR="/Users/tkefauver/mp/MonkeyPaste.Avalonia.Desktop/bin/Debug/net8.0/osx-x64/MonkeyPaste.app/Contents/MacOS/dat"
DAT_DIR="/Users/tkefauver/mp/MonkeyPaste.Avalonia.Desktop/bin/x64/Debug/net8.0/osx-x64/publish/MonkeyPaste.app/Contents/MacOS/dat"
CORE_OLE_GUID="cf2ec03f-9edd-45e9-a605-2a2df71e03bd"
CORE_ANN_GUID="ecde8e7c-30cf-47ef-a6a9-8f7f439b0a31"

rm -fr $DAT_DIR
mkdir $DAT_DIR 

cd "/Users/tkefauver/mp/Plugins/CoreOleHandler"

rm -fr bin
rm -fr obj

dotnet build -r osx-x64

rm -fr CoreOleHandler
cp -a "bin/Debug/net8.0/osx-x64/." "CoreOleHandler"
zip -rq "$CORE_OLE_GUID.zip" "CoreOleHandler"
mv "$CORE_OLE_GUID.zip" "$DAT_DIR"

echo "CoreOleHandler bundled to $DAT_DIR/$CORE_OLE_GUID.zip"

cd "/Users/tkefauver/mp/Plugins/CoreAnnotator"

rm -fr bin
rm -fr obj

dotnet build -r osx-x64

rm -fr CoreAnnotator
cp -a "bin/Debug/netstandard2.0/osx-x64/." "CoreAnnotator"
zip -rq "$CORE_ANN_GUID.zip" "CoreAnnotator"
mv "$CORE_ANN_GUID.zip" "$DAT_DIR"

echo "CoreOleHandler bundled to $DAT_DIR/$CORE_ANN_GUID.zip"

echo "DONE"
