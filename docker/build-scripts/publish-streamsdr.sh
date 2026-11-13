#!/bin/bash

echo "Publishing StreamSDR"

dotnet publish "/build/src/StreamSDR.csproj" -c Release -o /build/app
