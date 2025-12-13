#!/bin/bash

echo "Setting up non-root user"

# Create a non-root user with an explicit UID and add permission to access the /app folder

apt-get install -y --no-install-recommends adduser
adduser -u 5678 --disabled-password --gecos "" appuser
chown -R appuser /app
