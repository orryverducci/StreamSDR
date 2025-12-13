#!/usr/bin/env bash

# Set script to exit if a command fails
set -euo pipefail

# Get the directory of the build script
SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)

# Check dotnet is installed
if [ ! -x "$(command -v dotnet)" ]
then
    echo "ERROR: dotnet is not installed or could not be found"
    exit 1
fi

# Run the build application
dotnet run --project "$SCRIPT_DIR/build/Build.csproj" -- "$@"
