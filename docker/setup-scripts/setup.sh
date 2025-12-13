#!/bin/bash

# Set script to exit if a command fails
set -euo pipefail

# Update package list

apt-get update

# Install dependencies

source /tmp/scripts/install-dependencies-rtlsdr.sh
source /tmp/scripts/install-dependencies-sdrplay.sh

# Create a non-root user with an explicit UID and add permission to access the /app folder

source /tmp/scripts/setup-user.sh

# Clean up package list

rm -rf /var/lib/apt/lists/*
