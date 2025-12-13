#!/bin/bash

# Set script to exit if a command fails
set -euo pipefail

# Update package list

apt-get update

# Detect architecture
if [ -z ${TARGETARCH+x} ]; then

    PLATFORM=$(uname -m)

    if [[ $PLATFORM == x86_64* ]]; then
        export ARCH="x86_64"
    elif [[ $PLATFORM = aarch64 ]] || [[ $PLATFORM == arm64* ]]; then
        export ARCH="aarch64"
    else
        echo "Unsupported platform ($TARGETARCH)"
        exit 1
    fi

else

    if [[ $TARGETARCH == amd64 ]]; then
        export ARCH="x86_64"
    elif [[ $TARGETARCH = arm64 ]]; then
        export ARCH="aarch64"
    else
        echo "Unsupported platform ($TARGETARCH)"
        exit 1
    fi

fi

# Prepare runtime dependencies

source /build/scripts/download-s6.sh
source /build/scripts/download-sdrplay.sh

# Publish the app without the app host, which is not required by the runtime Docker image

source /build/scripts/publish-streamsdr.sh

# Clean up package list

rm -rf /var/lib/apt/lists/*
