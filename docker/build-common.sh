#!/bin/bash
echo "ROOT       = $ROOT"
echo "APPNAME    = $APPNAME"
echo "BUILD_PATH = $BUILD_PATH"

if [ -e "$BUILD_PATH" ]
then
    rm -rf "$BUILD_PATH"
fi

dotnet publish "$ROOT/app/$APPNAME/$APPNAME.csproj" -c Release -o "$BUILD_PATH"