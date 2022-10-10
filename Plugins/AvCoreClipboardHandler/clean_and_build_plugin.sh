PROJ_DIR=`pwd`
SLN_DIR=$(cd ../..; pwd)
PLUGIN_NAME=`basename $PROJ_DIR`
RUN_NET6_DIR="$SLN_DIR/MonkeyPaste.Avalonia/bin/Debug/net6.0/Plugins/$PLUGIN_NAME"
RUN_NET6_WIN_DIR="$SLN_DIR/MonkeyPaste.Avalonia/bin/Debug/net6.0-windows/Plugins/$PLUGIN_NAME"
PLUGIN_NET6_DIR="$PROJ_DIR/bin/Debug/net6.0"
# remove this plugins old folder from target

rm -fr $RUN_NET6_DIR
rm -fr $RUN_NET6_WIN_DIR

# clean for sanity
dotnet restore

# remove obj and bin to avoid 'error MSB4018: The "ResolvePackageAssets" task failed unexpectedly. '
rm -fr obj
rm -fr bin

# build plugin 
dotnet build -property:SolutionDir=$SLN_DIR

# copy plugin project and bin files to both targets
mkdir $RUN_NET6_DIR
mkdir $RUN_NET6_WIN_DIR

cp -fr $PROJ_DIR/* $RUN_NET6_DIR
cp -fr $PROJ_DIR/* $RUN_NET6_WIN_DIR

cp -fr $PLUGIN_NET6_DIR/* $RUN_NET6_DIR
cp -fr $PLUGIN_NET6_DIR/* $RUN_NET6_WIN_DIR


echo "DONE"
