#!/bin/sh
echo "Build Reset..."
cd "../../../../"
rm -fr Plugins/CoreOleHandler/obj/
rm -fr Plugins/CoreOleHandler/bin/

rm -fr Plugins/CoreAnnotator/obj/
rm -fr Plugins/CoreAnnotator/bin/

rm -fr Common/MonkeyPaste.Common/obj/
rm -fr Common/MonkeyPaste.Common/bin/

rm -fr Common/MonkeyPaste.Common.Avalonia/obj/
rm -fr Common/MonkeyPaste.Common.Avalonia/bin/

rm -fr Common/MonkeyPaste.Common.Plugin/obj/
rm -fr Common/MonkeyPaste.Common.Plugin/bin/

rm -fr MonkeyPaste.Avalonia.iOS/bin/
rm -fr MonkeyPaste.Avalonia.iOS/obj/

rm -fr MonkeyPaste.Avalonia/obj/
rm -fr MonkeyPaste.Avalonia/bin/

rm -fr MonkeyPaste/obj/
rm -fr MonkeyPaste/bin/
 
cd -
echo "DONE"
