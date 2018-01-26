#!/usr/bin/env bash

##########################################################################
# This is the Cake bootstrapper script for Linux and OS X.
# This script assumes the dotnet core runtime 1.0.x is already installed.
##########################################################################

# Define directories.
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/tools
if [ "$CAKE_VERSION" == "" ]; then
    CAKE_VERSION=0.21.1
fi

CAKE_DLL=$TOOLS_DIR/Cake.CoreCLR/$CAKE_VERSION/Cake.dll

# Define default arguments.
SCRIPT="build.cake"
TARGET="Default"
CONFIGURATION="Release"
VERBOSITY="verbose"
DRYRUN=
SHOW_VERSION=false
SCRIPT_ARGUMENTS=()

# Parse arguments.
for i in "$@"; do
    case $1 in
        -s|--script) SCRIPT="$2"; shift ;;
        -t|--target) TARGET="$2"; shift ;;
        -c|--configuration) CONFIGURATION="$2"; shift ;;
        -v|--verbosity) VERBOSITY="$2"; shift ;;
        -d|--dryrun) DRYRUN="--dryrun" ;;
        --version) SHOW_VERSION=true ;;
        --) shift; SCRIPT_ARGUMENTS+=("$@"); break ;;
        *) SCRIPT_ARGUMENTS+=("$1") ;;
    esac
    shift
done

# Make sure the tools folder exist.
if [ ! -d "$TOOLS_DIR" ]; then
    mkdir -p "$TOOLS_DIR"
fi

# Restore tools from NuGet.
if [ ! -f "$CAKE_DLL" ]; then
    mkdir -p "$TOOLS_DIR/Cake.CoreCLR/$CAKE_VERSION"
    curl -Lsfo Cake.CoreCLR.zip "https://www.nuget.org/api/v2/package/Cake.CoreCLR/$CAKE_VERSION" && unzip -q Cake.CoreCLR.zip -d "$TOOLS_DIR/Cake.CoreCLR/$CAKE_VERSION" && rm -f Cake.CoreCLR.zip
    if [ $? -ne 0 ]; then
        echo "An error occured while installing Cake."
        exit 1
    fi
fi

# Make sure that Cake has been installed.
if [ ! -f "$CAKE_DLL" ]; then
    echo "Could not find Cake.dll at '$CAKE_DLL'."
    exit 1
fi

# Start Cake
if $SHOW_VERSION; then
    exec dotnet "$CAKE_DLL" --version
else
    exec dotnet "$CAKE_DLL" $SCRIPT --verbosity=$VERBOSITY --configuration=$CONFIGURATION --target=$TARGET $DRYRUN "${SCRIPT_ARGUMENTS[@]}"
fi