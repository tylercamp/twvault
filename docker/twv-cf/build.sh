#!/bin/bash

export ROOT="../.."
export BUILD_PATH="$PWD/out"
export APPNAME="TW.ConfigurationFetcher"
export DOCKER_PATH="$PWD"
export DOCKER_NAME="twv-cf"
export DOCKER_TAG="$1"

sh ../build-common.sh