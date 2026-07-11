# Base .NET development image.
FROM mcr.microsoft.com/devcontainers/dotnet:10.0

# Install common developer utilities.
RUN apt-get update && apt-get install -y \
    bat \
    curl \
    fd-find \
    fzf \
    git \
    git-lfs \
    jq \
    less \
    ripgrep \
    tree \
    unzip \
    vim \
    wget \
    zip \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Create a symbolic link for fd-find to be accessible as 'fd'.
RUN ln -s "$(which fdfind)" /usr/local/bin/fd

# Enable Git LFS support.
RUN git lfs install

# Configure PowerShell profile directory.
RUN mkdir -p /home/vscode/.config/powershell

# Copy custom PowerShell profile into the container.
COPY .devcontainer/dotfiles/powershell-profile.ps1 /home/vscode/.config/powershell/Microsoft.PowerShell_profile.ps1

# Ensure NuGet config directory exists and is owned by vscode.
RUN mkdir -p /home/vscode/.nuget/NuGet \
    && chown -R vscode:vscode /home/vscode/.nuget

# Pre-create volume mount points with correct ownership so post-create.sh
# can write into them without requiring chown at runtime.
RUN mkdir -p \
    /home/vscode/.claude \
    /home/vscode/.claude-runtime/plugins \
    /home/vscode/.claude-runtime/marketplaces \
    /home/vscode/.claude-runtime/cache \
    && chown -R vscode:vscode \
    /home/vscode/.claude \
    /home/vscode/.claude-runtime

# Ensure vscode user owns their home directory.
RUN chown -R vscode:vscode /home/vscode
