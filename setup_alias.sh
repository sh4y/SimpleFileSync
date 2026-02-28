#!/bin/zsh

# Find the absolute path to the directory containing this script
DIR="$(cd "$(dirname "$0")" && pwd)"

# Build the project to ensure the executable exists
echo "Building SimpleFileSync..."
cd "$DIR"
dotnet build -c Release

# Path to the compiled executable DLL (use Debug since standard dotnet build outputs there)
BINARY_PATH="$DIR/bin/Debug/net9.0/SimpleFileSync.dll"

# --- Update ~/.zshrc ---
ZSHRC_FILE=~/.zshrc

# Remove the old alias logic if it exists (for safe re-runs)
# It deletes from the signature comment down to the closing brace
sed -i '' '/# Added by SimpleFileSync setup script/,/^}/d' "$ZSHRC_FILE" 2>/dev/null || true

# Inject the alias
echo "" >> "$ZSHRC_FILE"
echo "# Added by SimpleFileSync setup script" >> "$ZSHRC_FILE"
echo "unalias sfs 2>/dev/null" >> "$ZSHRC_FILE"
echo "function sfs {" >> "$ZSHRC_FILE"
echo "  local EXE_PATH=\"$BINARY_PATH\"" >> "$ZSHRC_FILE"
echo "  local DOTNET_CMD=\"\$(command -v dotnet)\"" >> "$ZSHRC_FILE"
echo "  if [[ -z \"\$DOTNET_CMD\" ]]; then echo 'Error: dotnet not found in PATH!'; return 1; fi" >> "$ZSHRC_FILE"
echo "  local RUN_IN_BG=0" >> "$ZSHRC_FILE"
echo "  local NEW_ARGS=()" >> "$ZSHRC_FILE"
echo "  for arg in \"\$@\"; do" >> "$ZSHRC_FILE"
echo "    if [[ \"\$arg\" == \"-d\" || \"\$arg\" == \"--daemon\" ]]; then" >> "$ZSHRC_FILE"
echo "      RUN_IN_BG=1" >> "$ZSHRC_FILE"
echo "    else" >> "$ZSHRC_FILE"
echo "      NEW_ARGS+=(\"\$arg\")" >> "$ZSHRC_FILE"
echo "    fi" >> "$ZSHRC_FILE"
echo "  done" >> "$ZSHRC_FILE"
echo "" >> "$ZSHRC_FILE"
echo "  if [[ \"\$1\" == \"server\" || \$RUN_IN_BG -eq 1 ]]; then" >> "$ZSHRC_FILE"
echo "    echo \"Starting SimpleFileSync in the background...\"" >> "$ZSHRC_FILE"
echo "    nohup \"\$DOTNET_CMD\" \"\$EXE_PATH\" \"\${NEW_ARGS[@]}\" >/dev/null 2>&1 &" >> "$ZSHRC_FILE"
echo "    echo \"Running (PID \$!)\"" >> "$ZSHRC_FILE"
echo "  else" >> "$ZSHRC_FILE"
echo "    \"\$DOTNET_CMD\" \"\$EXE_PATH\" \"\${NEW_ARGS[@]}\"" >> "$ZSHRC_FILE"
echo "  fi" >> "$ZSHRC_FILE"
echo "}" >> "$ZSHRC_FILE"

echo "Successfully injected 'sfs' function into ~/.zshrc"
echo "Please run 'source ~/.zshrc' or open a new terminal window to use the command."
