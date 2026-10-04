#!/usr/bin/env sh
set -e

# Fast build & Native AOT publish script for local development

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "==> Building solution in Debug mode..."
dotnet build "$REPO_ROOT/NativeAotCliTemplate.sln" -c Debug

echo "==> Running tests..."
dotnet test "$REPO_ROOT/NativeAotCliTemplate.sln" -c Debug --no-build

# Detect current platform RID
OS_NAME="$(uname -s)"
ARCH_NAME="$(uname -m)"

case "$OS_NAME" in
    Linux) OS="linux" ;;
    Darwin) OS="osx" ;;
    *) echo "Unsupported OS: $OS_NAME"; exit 1 ;;
esac

case "$ARCH_NAME" in
    x86_64|amd64) ARCH="x64" ;;
    arm64|aarch64) ARCH="arm64" ;;
    *) echo "Unsupported Arch: $ARCH_NAME"; exit 1 ;;
esac

RID="${OS}-${ARCH}"
OUTPUT_DIR="$REPO_ROOT/publish/$RID"

echo "==> Publishing Native AOT binary for $RID..."
dotnet publish "$REPO_ROOT/src/NativeAotCli/NativeAotCli.csproj" -c Release -r "$RID" -o "$OUTPUT_DIR"

BINARY="$OUTPUT_DIR/aotcli"
if [ -f "$BINARY" ]; then
    echo "==> Publish successful!"
    ls -lh "$BINARY"
    echo ""
    echo "==> Running sanity test:"
    "$BINARY" info
fi
