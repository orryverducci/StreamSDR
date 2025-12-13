#!/bin/bash

echo "Publishing StreamSDR"

apt-get install -y --no-install-recommends clang zlib1g-dev

dotnet publish "/build/src/StreamSDR.csproj" -c Release -o /build/app
