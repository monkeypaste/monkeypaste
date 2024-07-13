#!/bin/bash
clear
APP_PATH="/Users/tkefauver/mp/MonkeyPaste.Avalonia.iOS/bin/Debug/net8.0-ios/ios-arm64/MonkeyPaste.Avalonia.iOS.app"
SIGNING_IDENTITY="Apple Development: thomas kefauver (MY7R67BXWM)" # matches Keychain Access certificate name

#ENTITLEMENTS=`pwd`
#ENTITLEMENTS="${ENTITLEMENTS}/Entitlements.xcent"
ENTITLEMENTS="/Users/tkefauver/mp/Build/_build/scripts/ios/Entitlements.xcent"

rm ${ENTITLEMENTS}

cat > "${ENTITLEMENTS}" <<- EOM
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
	<key>application-identifier</key>
	<string>3382GDS46D.com.Monkey.MonkeyPaste</string>
	<key>com.apple.developer.team-identifier</key>
	<string>3382GDS46D</string>
	<key>get-task-allow</key>
	<true/>
</dict>
</plist>
EOM


find "$APP_PATH"|while read fname; do
    if [[ -f $fname ]]; then
        echo "[INFO] Signing $fname"
        codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$fname"
    fi
done

echo "[INFO] Signing app file"

codesign --force --timestamp --options=runtime --entitlements "$ENTITLEMENTS" --sign "$SIGNING_IDENTITY" "$APP_PATH"