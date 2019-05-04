#!/bin/bash
if [ -z "$1" ]
then
    echo "Version number must be provided!"
    exit 1
fi

cd twv-app
bash build.sh "$1"

cd ../twv-mdf
bash build.sh "$1"

cd ../twv-cf
bash build.sh "$1"

cd ../twv-migrate
bash build.sh "$1"