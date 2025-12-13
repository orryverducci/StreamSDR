#!/bin/bash

# Set script to exit if a command fails
set -euo pipefail

# Update package list

apt-get update

# Install dependencies

source /tmp/scripts/install-dependencies-rtlsdr.sh
source /tmp/scripts/install-dependencies-sdrplay.sh

# Create a non-root user with an explicit UID and add permission to access the /app folder

apt-get install -y --no-install-recommends adduser
adduser -u 5678 --disabled-password --gecos "" appuser
chown -R appuser /app

# Clean up package list

rm -rf /var/lib/apt/lists/*
