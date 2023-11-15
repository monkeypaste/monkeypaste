#!/bin/sh

rm -fvr /Users/tkefauver/mp/Plugins/CoreOleHandler/obj/
rm -fvr /Users/tkefauver/mp/Plugins/CoreAnnotator/obj/
rm -fr /Users/tkefauver/mp/MonkeyPaste.Avalonia.Desktop/bin/
rm -fvr /Users/tkefauver/mp/MonkeyPaste.Avalonia.Desktop/obj/
rm -fr /Users/tkefauver/mp/MonkeyPaste.Avalonia.Desktop/bin/
rm -fr /Users/tkefauver/mp/MonkeyPaste.Avalonia/obj/
rm -fr /Users/tkefauver/mp/MonkeyPaste.Avalonia/bin/
rm -fr /Users/tkefauver/mp/Common/MonkeyPaste.Common/obj/
rm -fr /Users/tkefauver/mp/Common/MonkeyPaste.Common/bin/
rm -fr /Users/tkefauver/mp/Common/MonkeyPaste.Common.Avalonia/obj/
rm -fr /Users/tkefauver/mp/Common/MonkeyPaste.Common.Avalonia/bin/

if [ "$2" = "reset" ]; then 
cd "/Users/tkefauver/.local/share"
echo pwd
rm -r MonkeyPaste_DEBUG/
echo -n "RESET DONE"
fi

