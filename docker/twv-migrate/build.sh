#!/bin/bash

export ROOT="../.."
export BUILD_PATH="$PWD/out"
export APPNAME="TW.Vault.Migration"
export DOCKER_PATH="$PWD"
export DOCKER_NAME="twv-migrate"
export DOCKER_TAG="$1"

sh ../build-common.sh