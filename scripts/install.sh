#!/usr/bin/env sh
set -e

# Native AOT CLI Template - Cross-Platform Unix Installer
# Supports Linux and macOS (x64, arm64)

APP_NAME="cli"
REPO="${REPO:-"ryanrodemoyer/csharp-dotnet-cli-aot-starter"}"
VERSION="${VERSION:-"latest"}"
INSTALL_DIR="${INSTALL_DIR:-"$HOME/.local/bin"}"

echo "=========================================="
echo "  Installing $APP_NAME"
echo "=========================================="

# 1. Detect OS
OS_NAME="$(uname -s)"
case "$OS_NAME" in
    Linux)
        OS="linux"
        ;;
    Darwin)
        OS="osx"
        ;;
    *)
        echo "Error: Unsupported operating system '$OS_NAME'." >&2
        exit 1
        ;;
esac

# 2. Detect Architecture
ARCH_NAME="$(uname -m)"
case "$ARCH_NAME" in
    x86_64|amd64)
        ARCH="x64"
        ;;
    arm64|aarch64)
        ARCH="arm64"
        ;;
    *)
        echo "Error: Unsupported architecture '$ARCH_NAME'." >&2
        exit 1
        ;;
esac

RID="${OS}-${ARCH}"
echo "Detected platform: $RID"

# Ensure destination directory exists
mkdir -p "$INSTALL_DIR"

# 3. Check if local source build is available
SCRIPT_DIR="$(cd "$(dirname "$0")" 2>/dev/null && pwd || echo "")"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." 2>/dev/null && pwd || echo "")"

if [ -f "$REPO_ROOT/src/cli/cli.csproj" ] && command -v dotnet >/dev/null 2>&1; then
    echo "Found local project source. Building Native AOT binary locally for $RID..."
    dotnet publish "$REPO_ROOT/src/cli/cli.csproj" -c Release -r "$RID" -o "$REPO_ROOT/publish/$RID" --nologo
    cp "$REPO_ROOT/publish/$RID/$APP_NAME" "$INSTALL_DIR/$APP_NAME"
else
    # Remote download from releases
    echo "Downloading pre-built release for $RID..."
    TMP_DIR="$(mktemp -d)"
    trap 'rm -rf "$TMP_DIR"' EXIT

    ARCHIVE_NAME="${APP_NAME}-${RID}.tar.gz"
    if [ "$VERSION" = "latest" ]; then
        DOWNLOAD_URL="https://github.com/${REPO}/releases/latest/download/${ARCHIVE_NAME}"
    else
        DOWNLOAD_URL="https://github.com/${REPO}/releases/download/${VERSION}/${ARCHIVE_NAME}"
    fi

    echo "Fetching: $DOWNLOAD_URL"
    if command -v curl >/dev/null 2>&1; then
        curl -fsSL "$DOWNLOAD_URL" -o "$TMP_DIR/$ARCHIVE_NAME" || {
            echo "Failed to download $DOWNLOAD_URL. If you are building locally, ensure .NET 10 SDK is installed." >&2
            exit 1
        }
    elif command -v wget >/dev/null 2>&1; then
        wget -qO "$TMP_DIR/$ARCHIVE_NAME" "$DOWNLOAD_URL" || {
            echo "Failed to download $DOWNLOAD_URL. If you are building locally, ensure .NET 10 SDK is installed." >&2
            exit 1
        }
    else
        echo "Error: Neither curl nor wget was found." >&2
        exit 1
    fi

    tar -xzf "$TMP_DIR/$ARCHIVE_NAME" -C "$TMP_DIR"
    cp "$TMP_DIR/$APP_NAME" "$INSTALL_DIR/$APP_NAME"
fi

chmod +x "$INSTALL_DIR/$APP_NAME"

echo ""
echo "Successfully installed $APP_NAME to $INSTALL_DIR/$APP_NAME"

# Check if INSTALL_DIR is in PATH
case ":$PATH:" in
    *":$INSTALL_DIR:"*) ;;
    *)
        echo ""
        echo "Note: '$INSTALL_DIR' is not in your current PATH."
        echo "Add the following line to your shell profile (~/.bashrc, ~/.zshrc, etc.):"
        echo "  export PATH=\"\$PATH:$INSTALL_DIR\""
        ;;
esac

echo ""
echo "Test run:"
"$INSTALL_DIR/$APP_NAME" info
