#!/bin/bash

export ROOT="../.."
export BUILD_PATH="$PWD/out"
export APPNAME="TW.Vault.App"
export DOCKER_PATH="$PWD"
export DOCKER_NAME="twv-app"
export DOCKER_TAG="$1"

bash ../build-common.sh