#!/usr/bin/env bash

set -e

echo "Running post-create setup..."

USER_HOME="/home/vscode"

export GIT_CONFIG_GLOBAL=/home/vscode/.config/git/config
mkdir -p "$(dirname "$GIT_CONFIG_GLOBAL")"

# ---------------------------------------------------------------------------
# Zsh / Oh My Zsh
# ---------------------------------------------------------------------------

# Install Oh My Zsh (unattended) as the vscode user.
if [ ! -d "$USER_HOME/.oh-my-zsh" ]; then
    sudo -u vscode sh -c \
        "$(curl -fsSL https://raw.githubusercontent.com/ohmyzsh/ohmyzsh/master/tools/install.sh)" "" --unattended
fi

# Set zsh as default shell.
sudo chsh -s /usr/bin/zsh vscode || true

# Write files as root then chown, since sudoers does not permit `sudo -u vscode tee`.
# Configure Oh My Posh for zsh.
cat > "$USER_HOME/.oh-my-posh.zsh" <<'EOF'
eval "$(oh-my-posh init zsh)"
EOF
chown vscode:vscode "$USER_HOME/.oh-my-posh.zsh"

# Append to .zshrc only once.
if ! grep -q "oh-my-posh.zsh" "$USER_HOME/.zshrc" 2>/dev/null; then
    cat >> "$USER_HOME/.zshrc" <<'EOF'

source ~/.oh-my-posh.zsh

# CSharpier installed to /root/.dotnet/tools (not on vscode user's PATH by default)
export PATH="$PATH:/root/.dotnet/tools"

# CLI completions (silent if completion files not yet available).
if [ -f /usr/share/zsh/site-functions/_dotnet ]; then
    source /usr/share/zsh/site-functions/_dotnet
fi

complete -o nospace -C /usr/bin/git git 2>/dev/null || true

# Aliases
alias ll="ls -alF"
alias gs="git status"

EOF
    chown vscode:vscode "$USER_HOME/.zshrc"
fi

# ---------------------------------------------------------------------------
# dotnet global tools
# ---------------------------------------------------------------------------

# Ensure global dotnet tools are on PATH for the remainder of this script.
export PATH="$PATH:/home/vscode/.dotnet/tools"

echo "Installing dotnet global tools..."

# Under `set -e` this aborts the entire script on any re-run (e.g. rebuild).
# This helper installs if absent, updates if present, and never fails the script.
dotnet_tool_ensure() {
    local tool="$1"
    if dotnet tool list -g | grep -q "^${tool}\b"; then
        dotnet tool update -g "$tool" 2>&1 || true
    else
        dotnet tool install -g "$tool" 2>&1 || true
    fi
}

dotnet_tool_ensure dotnet-ef
dotnet_tool_ensure dotnet-format
dotnet_tool_ensure dotnet-monitor
dotnet_tool_ensure dotnet-outdated-tool
dotnet_tool_ensure dotnet-script
dotnet_tool_ensure dotnet-trace
dotnet_tool_ensure dotnet-counters
dotnet_tool_ensure dotnet-dump

# ---------------------------------------------------------------------------
# Language servers
# ---------------------------------------------------------------------------

echo "Installing language servers..."

# Python language server.
if ! command -v pyright-langserver >/dev/null 2>&1; then
    npm install -g pyright || true
fi

# ---------------------------------------------------------------------------
# Claude Code environment
# ---------------------------------------------------------------------------

echo "Configuring Claude Code environment..."

# postCreateCommand runs as the remoteUser (vscode), not root. Named volumes may
# be mounted with root ownership if they pre-existed before the Dockerfile fix.
# Use sudo (runs as root) to create and fix ownership of the volume directories,
# then subsequent operations run as vscode without issue.
sudo mkdir -p \
    /home/vscode/.claude-runtime/plugins \
    /home/vscode/.claude-runtime/marketplaces \
    /home/vscode/.claude-runtime/cache \
    /home/vscode/.claude/plugins \
    /home/vscode/.claude/marketplaces \
    /home/vscode/.claude/cache

sudo chown -R vscode:vscode \
    /home/vscode/.claude-runtime \
    /home/vscode/.claude/plugins \
    /home/vscode/.claude/marketplaces \
    /home/vscode/.claude/cache

# Redirect volatile state into the runtime volume.
rm -rf /home/vscode/.claude/plugins
ln -sfn /home/vscode/.claude-runtime/plugins      /home/vscode/.claude/plugins
rm -rf /home/vscode/.claude/marketplaces
ln -sfn /home/vscode/.claude-runtime/marketplaces /home/vscode/.claude/marketplaces
rm -rf /home/vscode/.claude/cache
ln -sfn /home/vscode/.claude-runtime/cache        /home/vscode/.claude/cache

# Copy workspace skills into the volume (no-clobber so user edits are preserved).
if [ -d "/workspace/.claude-skills" ]; then
    mkdir -p /home/vscode/.claude/skills
    cp -rn /workspace/.claude-skills/. /home/vscode/.claude/skills/
fi

# ---------------------------------------------------------------------------
# Configuring Claude Marketplace and adding Code Plugins
# ---------------------------------------------------------------------------

echo "Installing Claude Code plugins..."

if command -v claude > /dev/null 2>&1; then
    # Register the official Anthropic marketplace if not already present.
    claude plugin marketplace add anthropics/claude-plugins-official 2>/dev/null || true

    # Now update and install.
    claude plugin marketplace update claude-plugins-official || true
    claude plugin install csharp-lsp@claude-plugins-official  || true
    claude plugin install pyright-lsp@claude-plugins-official || true
else
    echo "claude CLI not found — skipping plugin installs."
fi

# ---------------------------------------------------------------------------
# OpenCode config
# ---------------------------------------------------------------------------

mkdir -p /home/vscode/.config/opencode
cp .devcontainer/dotfiles/opencode.jsonc /home/vscode/.config/opencode/opencode.jsonc
chown vscode:vscode /home/vscode/.config/opencode/opencode.jsonc

# ---------------------------------------------------------------------------
# Pre-commit hooks
# ---------------------------------------------------------------------------

if command -v pip > /dev/null 2>&1; then
    pip install --quiet pre-commit codespell 2>/dev/null || true
fi

# Install the pre-commit config to the workspace root.
if [ -f .devcontainer/dotfiles/pre-commit-config.yaml ] && [ ! -f /workspace/.pre-commit-config.yaml ]; then
    cp .devcontainer/dotfiles/pre-commit-config.yaml /workspace/.pre-commit-config.yaml
    chown vscode:vscode /workspace/.pre-commit-config.yaml
    echo "Pre-commit config deployed to /workspace/.pre-commit-config.yaml"
    echo "  Activate with: cd /workspace && pre-commit install"
fi

# ---------------------------------------------------------------------------
# Solution restore
# ---------------------------------------------------------------------------

sln_count=$(find . -name "*.sln" -maxdepth 2 | wc -l)
if [ "$sln_count" -eq 0 ]; then
    echo "No .sln files found — skipping NuGet restore."
    echo "  (Run 'dotnet restore' manually or use /init to scaffold a project.)"
else
    find . -name "*.sln" -maxdepth 2 -print0 | while IFS= read -r -d '' sln; do
        echo "Restoring: $sln"
        dotnet restore "$sln" || echo "WARNING: restore failed for $sln — continuing."
    done
fi

# ---------------------------------------------------------------------------
# PowerShell profile
# ---------------------------------------------------------------------------

PWSH_PROFILE_DIR="/home/vscode/.config/powershell"
PWSH_PROFILE="$PWSH_PROFILE_DIR/Microsoft.PowerShell_profile.ps1"

mkdir -p "$PWSH_PROFILE_DIR"

# Write as root then chown, since sudoers does not permit `sudo -u vscode tee`.
if ! grep -q "oh-my-posh" "$PWSH_PROFILE" 2>/dev/null; then
    cat >> "$PWSH_PROFILE" <<'EOF'
oh-my-posh init pwsh | Invoke-Expression
EOF
    chown vscode:vscode "$PWSH_PROFILE"
fi

echo "Post-create complete."
