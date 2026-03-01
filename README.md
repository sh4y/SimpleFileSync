# Simple File Sync

A lightweight, Dropbox-like local file synchronization tool over the network. You can run both the Server (receiver) and the Client (sharer) out of the exact same application.

## How It Works

- **Server**: Listens for incoming file transfers and saves them identically out to a target directory.
- **Client**: Watches a directory for new or modified files. When a change happens, it automatically connects to the server and uploads the newly adjusted data.

## Getting Started

The easiest way to use Simple File Sync is by installing the `sfs` alias command. This allows you to run it from any directory on your machine.

### 1. Install the `sfs` Command
```bash
cd /Users/shay/shaybackup/SimpleFileSync
chmod +x setup_alias.sh
./setup_alias.sh
source ~/.zshrc
```

### 2. Run the Server (Receiver)
Run the server on the machine you want to act as the central storage (e.g., your Mac Mini running Docker). The server runs in the background automatically when using the `sfs` command.

```bash
# Basic setup (defaults to outputting to ./SyncTarget)
sfs server

# Custom target folder (e.g. Media Drive)
sfs server /workspace/storage/media
```

### 3. Run the Client (Sender)
Run the client on the machine you want to share files from. When you provide an IP address, it will send the files across your local network to that machine (default port `13890`).

```bash
# Send to a Server on another machine (e.g. your Mac Mini)
sfs client 192.168.1.100 /local/path/to/media

# Send to a Server on another machine, with a custom 5 second wait before transferring
sfs client 192.168.1.100 /local/path/to/media --delay 5

# Local testing ONLY (Sends to localhost 127.0.0.1)
sfs client ./SourceFolder

# Run the client cleanly in the background as a daemon
sfs client 192.168.1.100 /local/path/to/media -d

# Send all existing files in the directory immediately on first run, then watch for changes
sfs client 192.168.1.100 /local/path/to/media --init
```

## Quick Start (Testing on a Single Machine)
1. Open Terminal A: `sfs server ./TargetFolder`
2. Open Terminal B: `sfs client ./SourceFolder --delay 3`
3. Add a file into `./SourceFolder`. Watch it appear in `./TargetFolder` after 3 seconds!

## Command Reference

### Server
`sfs server [<target_folder>]`
* `<target_folder>`: (Optional) Path to store received files. Defaults to `./SyncTarget`.

### Client
`sfs client [<server_ip>] [<source_folder>] [options]`
* `<server_ip>`: (Optional) IP address of the server. Defaults to `127.0.0.1`.
* `<source_folder>`: (Optional) Path to folder you want to watch. Defaults to `./SyncSource`.

**Client Options:**
* `--delay <seconds>`: Number of seconds to wait before transferring a file after detecting it (Defaults to 15s).
* `--init`: Forces the client to immediately sync all existing files in the watched directory when it starts up.
* `-d` or `--daemon`: Run the `sfs` client in the background.
