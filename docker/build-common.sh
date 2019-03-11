#!/bin/bash
echo "ROOT        = $ROOT"
echo "APPNAME     = $APPNAME"
echo "BUILD_PATH  = $BUILD_PATH"
echo "DOCKER_TAG  = $DOCKER_TAG"
echo "DOCKER_PATH = $DOCKER_PATH"
echo "DOCKER_NAME = $DOCKER_NAME"

if [ -z "$DOCKER_TAG" ]
then
    echo "Must provide a tag!"
    exit 1
fi

if [ -e "$BUILD_PATH" ]
then
    rm -rf "$BUILD_PATH"
fi

dotnet publish "$ROOT/app/$APPNAME/$APPNAME.csproj" -c Release -o "$BUILD_PATH"
docker build -t "$DOCKER_NAME:$DOCKER_TAG" "$DOCKER_PATH"