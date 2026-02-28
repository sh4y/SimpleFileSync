using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public static class FileSyncClient
{
    private static readonly ConcurrentDictionary<string, DateTime> _lastSent = new ConcurrentDictionary<string, DateTime>();

    public static void RunClient(string serverIp, string sourceFolder, int delaySeconds)
    {
        // 9. Ensure source folder exists
        Directory.CreateDirectory(sourceFolder);
        Console.WriteLine($"[Client] Watching folder: {sourceFolder}");
        Console.WriteLine($"[Client] Sending changes to server at {serverIp}:{Program.PORT}");
        Console.WriteLine($"[Client] Wait clock set to {delaySeconds} seconds.");

        // 10. Set up FileSystemWatcher to monitor folder
        using var watcher = new FileSystemWatcher(sourceFolder);
        watcher.IncludeSubdirectories = true;
        watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size;

        // 11. Add event handlers for file Creation, Change, and Rename events (macOS heavily uses renames for screenshot creation)
        watcher.Created += (s, e) => OnFileChanged(e.FullPath, sourceFolder, serverIp, delaySeconds);
        watcher.Changed += (s, e) => OnFileChanged(e.FullPath, sourceFolder, serverIp, delaySeconds);
        watcher.Renamed += (s, e) => OnFileChanged(e.FullPath, sourceFolder, serverIp, delaySeconds);
        
        watcher.EnableRaisingEvents = true;

        Console.WriteLine("[Client] Running. Press Ctrl+C to stop.");
        new System.Threading.ManualResetEvent(false).WaitOne();
    }

    private static async void OnFileChanged(string fullPath, string rootFolder, string serverIp, int delaySeconds)
    {
        try
        {
            // 12. Skip directories (we only transfer files contextually)
            if (Directory.Exists(fullPath)) return;

            // Optional 12.5 Debounce immediate duplicates
            if (_lastSent.TryGetValue(fullPath, out var lastTime) && (DateTime.UtcNow - lastTime).TotalMilliseconds < 1000)
            {
                return;
            }
            _lastSent[fullPath] = DateTime.UtcNow;

            Console.WriteLine($"[Client] Detected change to: {fullPath}, starting {delaySeconds}s clock...");

            // Wait the configurable amount of seconds
            await Task.Delay(delaySeconds * 1000);
            
            Console.WriteLine($"[Client] {delaySeconds}s clock finished for: {fullPath}, checking size stability...");

            // 13. Wait for file size to stabilize (e.g., handling slow downloads)
            long lastSize = -1;
            int retries = 0;
            while (true)
            {
                await Task.Delay(1000); // Check every 1 second
                
                if (!File.Exists(fullPath)) return; // File was deleted or moved
                
                long currentSize = new FileInfo(fullPath).Length;
                
                if (currentSize == lastSize)
                {
                    try
                    {
                        // Test if the OS has fully unlocked it
                        using (var testFs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            break; // Stable and unlocked!
                        }
                    }
                    catch (IOException)
                    {
                        // Still locked by another process (e.g. downloading), wait and try again
                    }
                }
                
                lastSize = currentSize;
                retries++;

                if (retries > 3600) // Safety timeout after an hour (large downloads)
                {
                    Console.WriteLine($"[Client] Timeout waiting for {fullPath} to stabilize.");
                    return;
                }
            }

            // Update debounce tracker again so our own read and subsequent OS updates don't immediately re-trigger
            _lastSent[fullPath] = DateTime.UtcNow;
            
            Console.WriteLine($"[Client] File is stable: {fullPath}, starting transfer...");

            // 14. Determine relative path to recreate same structure on Server
            // Prefix the relative path with the machine name and the source folder's name 
            // so the server separates files by client and folder.
            string folderName = new DirectoryInfo(rootFolder).Name;
            string relativePath = Path.Combine(Environment.MachineName, folderName, Path.GetRelativePath(rootFolder, fullPath));

            // 15. Connect to Server
            using TcpClient client = new TcpClient(serverIp, Program.PORT);
            using var stream = client.GetStream();
            using var writer = new BinaryWriter(stream);
            
            // 16. Send the relative file path
            writer.Write(relativePath);
            
            // 17. Send the file length
            var fileInfo = new FileInfo(fullPath);
            writer.Write(fileInfo.Length);

            // 18. Send the file data
            using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                await fs.CopyToAsync(stream);
            }
            
            Console.WriteLine($"[Client] Sent: {relativePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client] Failed to send {fullPath}: {ex.Message}");
        }
    }
}
