#!/bin/bash
# from https://www.internalpointers.com/post/build-binary-deb-package-practical-guide

VERSION=1.0-18
FRAMEWORK=net8.0
ARCH=amd64
PLATFORM=linux-x64
EXE_NAME=MonkeyPaste.Desktop

FULL_NAME="Thomas Kefuver"
MY_EMAIL=thomask@monkeypaste.com
APP_NAME=MonkeyPaste
PACKAGE=monkeypaste

cd "../../../../MonkeyPaste.Desktop/"

PROJ_DIR=`pwd`
PUBLISH_DIR="${PROJ_DIR}/bin/Release/${FRAMEWORK}/${PLATFORM}/publish"
PACKAGE_NAME="${PACKAGE}_${VERSION}_${ARCH}"
PACKAGE_DIR="${PROJ_DIR}/packages/deb"
WORKING_DIR="${PACKAGE_DIR}/${PACKAGE_NAME}"

# publish release
if [ "$1" = "build" ]; then
	echo "BUILDING..."
	rm -fr bin/Release
	dotnet publish -c Release -f $FRAMEWORK
	echo "DONE"
fi

echo "PACKAGING..."

# move to working dir
mkdir -p $PACKAGE_DIR
cd $PACKAGE_DIR

# copy <publish_dir> to <package_dir>/usr/lib/<package_name_dir> 
rm -fr $WORKING_DIR
TARGET_LIB_DIR="/usr/lib/${PACKAGE}"
LIB_DIR="${WORKING_DIR}${TARGET_LIB_DIR}"
mkdir -p $LIB_DIR
cp -fa "${PUBLISH_DIR}/." "${LIB_DIR}/"

# rename exe to <package_dir>
mkdir -p "${WORKING_DIR}/usr/local/bin"
TARGET_EXE_PATH="/usr/local/bin/${PACKAGE}"
ln -s "${LIB_DIR}/${EXE_NAME}" "${WORKING_DIR}${TARGET_EXE_PATH}"

# create control file
mkdir "${WORKING_DIR}/DEBIAN"
touch "${WORKING_DIR}/DEBIAN/control"

cat > "${WORKING_DIR}/DEBIAN/control" <<- EOM
Package: ${PACKAGE}
Version: ${VERSION}
Architecture: ${ARCH}
Depends: bash, xclip, xdotool
Maintainer: ${FULL_NAME} <${MY_EMAIL}>
Description: A clipboard manager and more.
 MonkeyPaste is a clipboard manager, automation and productivity tool like no other. Enhancing your clipboard into an enriched and organized part of your workflow, featuring an intuitive and low-profile interface that supports text, files and images. Designed for flexibility, with an ever-growing community-driven libray of plugins.
EOM

# add post uninstall script (removes local storage)
#POSTRM_SCRIPT_PATH="${WORKING_DIR}/DEBIAN/postrm"
#cat > $POSTRM_SCRIPT_PATH <<- EOM
##!/bin/bash
#rm -fr $HOME/share/.local/MonkeyPaste
#EOM
#chmod +x $POSTRM_SCRIPT_PATH

# create icons
ICON_SVG_PATH=/home/tkefauver/mp/artwork/canva/monkey/Default.svg
ICONS_DIR="${WORKING_DIR}/usr/share/icons/hicolor"
ICON_FILE_NAME="${PACKAGE}.png"
mkdir -p $ICONS_DIR

# scalable icon
mkdir -p "${ICONS_DIR}/scalable/apps"
cp $ICON_SVG_PATH "${ICONS_DIR}/scalable/apps/${PACKAGE}.svg"

# fixed size icons
DIMENSIONS="8 16 22 24 32 36 42 48 64 72 96 128 192 256 512"
for dim in $DIMENSIONS; do
	OUTPUT_DIR="${ICONS_DIR}/${dim}x${dim}"
	mkdir -p $OUTPUT_DIR
	OUTPUT_PATH="${OUTPUT_DIR}/${ICON_FILE_NAME}"
	rsvg-convert -w $dim -h $dim -p 96 -d 96 -a $ICON_SVG_PATH -o $OUTPUT_PATH
done

# Create launcher file
LAUNCHER_DIR="${WORKING_DIR}/usr/share/applications"
mkdir -p $LAUNCHER_DIR
LAUNCHER_PATH="${LAUNCHER_DIR}/${PACKAGE}.desktop"
touch $LAUNCHER_PATH

cat > $LAUNCHER_PATH <<- EOM
[Desktop Entry]
Name=${APP_NAME}
Icon=${PACKAGE}
Type=Application
Categories=Utility;
Keywords=clipboard;automation;
Exec=${PACKAGE}
Terminal=false
Version=${VERSION}
Comment=A clipboard manager and more.
EOM

dpkg-deb --build --root-owner-group $PACKAGE_NAME

rm -fr $PACKAGE_NAME

echo "DONE"

if [ "$1" = "install" ] || [ "$2" = "install" ]; then
	echo "INSTALLING..."
	DEB_NAME="${PACKAGE_NAME}.deb"
	sudo dpkg -i $DEB_NAME
	echo "DONE"
fi
