#!/bin/zsh

# Find the absolute path to the directory containing this script
DIR="$(cd "$(dirname "$0")" && pwd)"

# Build the project to ensure the executable exists
echo "Building SimpleFileSync..."
cd "$DIR"
dotnet build -c Release

# Path to the compiled executable shim (adjust framework version if needed)
BINARY_PATH="$DIR/bin/Release/net9.0/SimpleFileSync"

# Add the bash function to .zshrc if it doesn't already exist
if grep -q "function sfs {" ~/.zshrc; then
    echo "Function 'sfs' already exists in ~/.zshrc"
else
    echo "" >> ~/.zshrc
    echo "# Added by SimpleFileSync setup script" >> ~/.zshrc
    echo "unalias sfs 2>/dev/null" >> ~/.zshrc
    echo "function sfs {" >> ~/.zshrc
    echo "  local EXE_PATH=\"$BINARY_PATH\"" >> ~/.zshrc
    echo "  local RUN_IN_BG=0" >> ~/.zshrc
    echo "  local NEW_ARGS=()" >> ~/.zshrc
    echo "  for arg in \"\$@\"; do" >> ~/.zshrc
    echo "    if [[ \"\$arg\" == \"-d\" || \"\$arg\" == \"--daemon\" ]]; then" >> ~/.zshrc
    echo "      RUN_IN_BG=1" >> ~/.zshrc
    echo "    else" >> ~/.zshrc
    echo "      NEW_ARGS+=(\"\$arg\")" >> ~/.zshrc
    echo "    fi" >> ~/.zshrc
    echo "  done" >> ~/.zshrc
    echo "" >> ~/.zshrc
    echo "  if [[ \"\$1\" == \"server\" || \$RUN_IN_BG -eq 1 ]]; then" >> ~/.zshrc
    echo "    echo \"Starting SimpleFileSync in the background...\"" >> ~/.zshrc
    echo "    nohup \"\$EXE_PATH\" \"\${NEW_ARGS[@]}\" >/dev/null 2>&1 &" >> ~/.zshrc
    echo "    echo \"Running (PID \$!)\"" >> ~/.zshrc
    echo "  else" >> ~/.zshrc
    echo "    \"\$EXE_PATH\" \"\${NEW_ARGS[@]}\"" >> ~/.zshrc
    echo "  fi" >> ~/.zshrc
    echo "}" >> ~/.zshrc
    echo "Successfully added 'sfs' function to ~/.zshrc"
    echo "Please run 'source ~/.zshrc' or open a new terminal window to use the command."
fi
