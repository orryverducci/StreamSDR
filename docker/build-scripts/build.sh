#!/bin/bash

# Update package list

apt-get update

# Publish the app without the app host, which is not required by the runtime Docker image

dotnet publish "/build/src/StreamSDR.csproj" /p:UseAppHost=false /p:PublishSingleFile=false -c Release -o /build/app

# Clean up package list

rm -rf /var/lib/apt/lists/*
