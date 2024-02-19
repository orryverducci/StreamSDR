#!/bin/bash

# Update package list

apt-get update

# Instal dependencies

sh install-dependencies-rtlsdr.sh

# Create a non-root user with an explicit UID and add permission to access the /app folder

adduser -u 5678 --disabled-password --gecos "" appuser
chown -R appuser /app

# Clean up package list

rm -rf /var/lib/apt/lists/*
