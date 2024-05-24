rem from https://stackoverflow.com/a/71149490/105028
@echo off
	
set SVG_PATH=../../../../../artwork/canva/monkey/Default.svg
set ICON_SET_PATH=../../../../../Scratch/iosTest/iosTest.iOS/Media.xcassets/AppIcons.appiconset
java -jar svg2png.jar -f %SVG_PATH% -o %ICON_SET_PATH% -c ios_icon.json
pause