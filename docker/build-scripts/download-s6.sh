#!/bin/bash

echo "Getting s6-overlay"

S6_OVERLAY_VERSION="3.1.6.2"

apt-get install -y --no-install-recommends tar wget xz-utils

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

# Download s6-overlay

wget --no-http-keep-alive -P /build/s6/archive https://github.com/just-containers/s6-overlay/releases/download/v${S6_OVERLAY_VERSION}/s6-overlay-noarch.tar.xz
wget --no-http-keep-alive -P /build/s6/archive https://github.com/just-containers/s6-overlay/releases/download/v${S6_OVERLAY_VERSION}/s6-overlay-${ARCH}.tar.xz

# Extract from the archive

mkdir /build/s6/output

tar -C /build/s6/output -Jxpf /build/s6/archive/s6-overlay-noarch.tar.xz
tar -C /build/s6/output -Jxpf /build/s6/archive/s6-overlay-${ARCH}.tar.xz
