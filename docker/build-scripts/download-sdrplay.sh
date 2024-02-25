#!/bin/bash

echo "Getting SDRplay API"

API_VERSION="3.14.0"

apt-get install -y --no-install-recommends wget patch

# Detect architecture

PLATFORM=$(uname -m)

if [[ $PLATFORM == x86_64* ]]; then
    ARCH="x86_64"
elif [[ $PLATFORM = aarch64 ]] || [[ $PLATFORM == arm64* ]]; then
    ARCH="aarch64"
else
    echo "Unsupported platform ($PLATFORM)"
    exit 1
fi

# Download SDRplay API

wget --no-http-keep-alive -P /build/sdrplay/package https://www.sdrplay.com/software/SDRplay_RSP_API-Linux-${API_VERSION}.run

# Extract files from the install package

bash /build/sdrplay/package/SDRplay_RSP_API-Linux-${API_VERSION}.run --noexec --target /build/sdrplay/installer

# Patch the install script

patch --verbose -N /build/sdrplay/installer/install_lib.sh /build/patches/sdrplay/install_lib.patch
