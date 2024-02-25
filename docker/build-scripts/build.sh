#!/bin/bash

# Update package list

apt-get update

# Prepare runtime dependencies

bash /build/scripts/download-s6.sh
bash /build/scripts/download-sdrplay.sh

# Publish the app without the app host, which is not required by the runtime Docker image

dotnet publish "/build/src/StreamSDR.csproj" /p:UseAppHost=false /p:PublishSingleFile=false -c Release -o /build/app

# Clean up package list

rm -rf /var/lib/apt/lists/*
